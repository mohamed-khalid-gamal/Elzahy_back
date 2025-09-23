using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Elzahy.Data;
using Elzahy.Services;
using Elzahy.Models;
using Elzahy.DTOs;
using Microsoft.AspNetCore.DataProtection;

namespace Elzahy.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<ITwoFactorService> _mockTwoFactorService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
        private readonly Mock<IDataProtector> _mockDataProtector;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            // Setup mocks
            _mockJwtService = new Mock<IJwtService>();
            _mockTwoFactorService = new Mock<ITwoFactorService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            _mockDataProtector = new Mock<IDataProtector>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            // Setup configuration
            var configurationData = new Dictionary<string, string>
            {
                ["JwtSettings:ExpirationInMinutes"] = "60",
                ["JwtSettings:TempTokenExpirationInMinutes"] = "5",
                ["App:BaseUrl"] = "https://localhost:7000",
                ["App:FrontendUrl"] = "http://localhost:4200"
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Setup data protection
            _mockDataProtectionProvider.Setup(x => x.CreateProtector("TempTokens"))
                .Returns(_mockDataProtector.Object);

            _authService = new AuthService(
                _context,
                _mockJwtService.Object,
                _mockTwoFactorService.Object,
                _mockEmailService.Object,
                _configuration,
                _mockDataProtectionProvider.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                Name = "Test User",
                Terms = true
            };

            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("test-access-token");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("test-refresh-token");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.Equal("test-access-token", result.Data.AccessToken);
            Assert.Equal("test-refresh-token", result.Data.RefreshToken);
            Assert.NotNull(result.Data.User);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentialsNo2FA_ReturnsTokens()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                TwoFactorEnabled = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("test-access-token");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("test-refresh-token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.RequiresTwoFactor);
            Assert.Equal("test-access-token", result.Data.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentialsWith2FA_ReturnsTempToken()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                TwoFactorEnabled = true,
                TwoFactorSecret = "TESTSECRET123456"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockDataProtector.Setup(x => x.Protect(It.IsAny<string>()))
                .Returns("protected-temp-token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.RequiresTwoFactor);
            Assert.Equal("protected-temp-token", result.Data.TempToken);
            Assert.Empty(result.Data.AccessToken);
        }

        [Fact]
        public async Task Setup2FAAsync_ValidUser_ReturnsQRCodeAndSecret()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var secret = "TESTSECRET123456789012345678901234";
            _mockTwoFactorService.Setup(x => x.GenerateSecret())
                .Returns(secret);
            _mockTwoFactorService.Setup(x => x.GenerateQrCodeUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("otpauth://totp/test");
            _mockTwoFactorService.Setup(x => x.GenerateQrCodeImage(It.IsAny<string>()))
                .Returns(new byte[] { 1, 2, 3 });
            _mockTwoFactorService.Setup(x => x.FormatSecretForDisplay(It.IsAny<string>()))
                .Returns("TEST SECR ET12 3456");

            // Act
            var result = await _authService.Setup2FAAsync(user.Id);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.Equal(secret, result.Data.SecretKey);
            Assert.Contains("data:image/png;base64,", result.Data.QrCodeImage);
            Assert.Equal("TEST SECR ET12 3456", result.Data.ManualEntryKey);
        }

        [Fact]
        public async Task Enable2FAAsync_ValidCode_EnablesAndReturnsRecoveryCodes()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                TwoFactorSecret = "TESTSECRET123456"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new Enable2FARequestDto { Code = "123456" };

            _mockTwoFactorService.Setup(x => x.ValidateTotp(It.IsAny<string>(), "123456"))
                .Returns(true);

            // Act
            var result = await _authService.Enable2FAAsync(user.Id, request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Success);
            Assert.NotEmpty(result.Data.RecoveryCodes);
            Assert.Equal(10, result.Data.RecoveryCodes.Count);

            // Verify user is updated
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.True(updatedUser!.TwoFactorEnabled);
        }

        [Fact]
        public async Task VerifyTwoFactorAsync_ValidTempTokenAndCode_ReturnsAccessToken()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                TwoFactorEnabled = true,
                TwoFactorSecret = "TESTSECRET123456"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new TempTokenVerifyRequestDto
            {
                TempToken = "temp-token",
                Code = "123456"
            };

            _mockDataProtector.Setup(x => x.Unprotect("temp-token"))
                .Returns($"{{\"UserId\":\"{user.Id}\",\"IssuedAt\":\"{DateTime.UtcNow:O}\"}}");
            _mockTwoFactorService.Setup(x => x.ValidateTotp("TESTSECRET123456", "123456"))
                .Returns(true);
            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("final-access-token");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("final-refresh-token");

            // Act
            var result = await _authService.VerifyTwoFactorAsync(request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.Equal("final-access-token", result.Data.AccessToken);
            Assert.Equal("final-refresh-token", result.Data.RefreshToken);
        }

        [Fact]
        public async Task VerifyRecoveryCodeAsync_ValidCode_ReturnsAccessTokenAndMarksCodeUsed()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                TwoFactorEnabled = true
            };
            var recoveryCode = new RecoveryCode
            {
                UserId = user.Id,
                CodeHash = BCrypt.Net.BCrypt.HashPassword("123-456"),
                IsUsed = false
            };
            user.RecoveryCodes.Add(recoveryCode);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new RecoveryCodeVerifyRequestDto
            {
                TempToken = "temp-token",
                RecoveryCode = "123-456"
            };

            _mockDataProtector.Setup(x => x.Unprotect("temp-token"))
                .Returns($"{{\"UserId\":\"{user.Id}\",\"IssuedAt\":\"{DateTime.UtcNow:O}\"}}");
            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("final-access-token");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("final-refresh-token");

            // Act
            var result = await _authService.VerifyRecoveryCodeAsync(request);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Data);
            Assert.Equal("final-access-token", result.Data.AccessToken);

            // Verify recovery code is marked as used
            var updatedCode = await _context.RecoveryCodes.FindAsync(recoveryCode.Id);
            Assert.True(updatedCode!.IsUsed);
            Assert.NotNull(updatedCode.UsedAt);
        }

        [Fact]
        public async Task Disable2FAAsync_ValidUser_DisablesAndRemovesRecoveryCodes()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Name = "Test User",
                TwoFactorEnabled = true,
                TwoFactorSecret = "TESTSECRET123456"
            };
            var recoveryCode = new RecoveryCode
            {
                UserId = user.Id,
                CodeHash = BCrypt.Net.BCrypt.HashPassword("123-456")
            };
            user.RecoveryCodes.Add(recoveryCode);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Disable2FAAsync(user.Id);

            // Assert
            Assert.True(result.Ok);
            Assert.True(result.Data);

            // Verify user is updated
            var updatedUser = await _context.Users.Include(u => u.RecoveryCodes).FirstAsync(u => u.Id == user.Id);
            Assert.False(updatedUser.TwoFactorEnabled);
            Assert.Null(updatedUser.TwoFactorSecret);
            Assert.Empty(updatedUser.RecoveryCodes);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}