using Xunit;
using Elzahy.Services;
using Microsoft.Extensions.Configuration;

namespace Elzahy.Tests.Services
{
    public class TwoFactorServiceTests
    {
        private readonly TwoFactorService _twoFactorService;
        private readonly IConfiguration _configuration;

        public TwoFactorServiceTests()
        {
            var configurationData = new Dictionary<string, string>
            {
                ["TwoFactor:Issuer"] = "Test App",
                ["TwoFactor:WindowSeconds"] = "30"
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            _twoFactorService = new TwoFactorService(_configuration);
        }

        [Fact]
        public void GenerateSecret_ShouldReturnValidBase32String()
        {
            // Act
            var secret = _twoFactorService.GenerateSecret();

            // Assert
            Assert.NotNull(secret);
            Assert.NotEmpty(secret);
            Assert.True(secret.Length >= 16); // Base32 encoded 20 bytes should be at least 16 chars
            Assert.Matches("^[A-Z2-7]+$", secret); // Base32 characters only
        }

        [Fact]
        public void GenerateCode_ShouldReturn6DigitCode()
        {
            // Act
            var code = _twoFactorService.GenerateCode();

            // Assert
            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
            Assert.True(int.TryParse(code, out _)); // Should be numeric
        }

        [Fact]
        public void ValidateCode_WithMatchingCodes_ShouldReturnTrue()
        {
            // Arrange
            var storedCode = "123456";
            var inputCode = "123456";

            // Act
            var result = _twoFactorService.ValidateCode(inputCode, storedCode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateCode_WithDifferentCodes_ShouldReturnFalse()
        {
            // Arrange
            var storedCode = "123456";
            var inputCode = "654321";

            // Act
            var result = _twoFactorService.ValidateCode(inputCode, storedCode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GenerateQrCodeUrl_ShouldReturnValidOtpAuthUrl()
        {
            // Arrange
            var userEmail = "test@example.com";
            var secret = "TESTSECRET123456";
            var issuer = "Test App";

            // Act
            var url = _twoFactorService.GenerateQrCodeUrl(userEmail, secret, issuer);

            // Assert
            Assert.NotNull(url);
            Assert.StartsWith("otpauth://totp/", url);
            Assert.Contains(Uri.EscapeDataString(issuer), url);
            Assert.Contains(Uri.EscapeDataString(userEmail), url);
            Assert.Contains($"secret={secret}", url);
            Assert.Contains($"issuer={Uri.EscapeDataString(issuer)}", url);
        }

        [Fact]
        public void GenerateQrCodeImage_ShouldReturnByteArray()
        {
            // Arrange
            var qrCodeUrl = "otpauth://totp/TestApp:test@example.com?secret=TESTSECRET123456&issuer=TestApp";

            // Act
            var imageBytes = _twoFactorService.GenerateQrCodeImage(qrCodeUrl);

            // Assert
            Assert.NotNull(imageBytes);
            Assert.True(imageBytes.Length > 0);
        }

        [Fact]
        public void GenerateTotp_WithSameSecret_ShouldReturnConsistentCode()
        {
            // Arrange
            var secret = _twoFactorService.GenerateSecret();

            // Act
            var code1 = _twoFactorService.GenerateTotp(secret);
            var code2 = _twoFactorService.GenerateTotp(secret);

            // Assert
            Assert.NotNull(code1);
            Assert.NotNull(code2);
            Assert.Equal(6, code1.Length);
            Assert.Equal(6, code2.Length);
            Assert.Equal(code1, code2); // Should be same within the same time window
        }

        [Fact]
        public void ValidateTotp_WithValidCode_ShouldReturnTrue()
        {
            // Arrange
            var secret = _twoFactorService.GenerateSecret();
            var code = _twoFactorService.GenerateTotp(secret);

            // Act
            var result = _twoFactorService.ValidateTotp(secret, code);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateTotp_WithInvalidCode_ShouldReturnFalse()
        {
            // Arrange
            var secret = _twoFactorService.GenerateSecret();
            var invalidCode = "000000";

            // Act
            var result = _twoFactorService.ValidateTotp(secret, invalidCode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FormatSecretForDisplay_ShouldFormatInGroupsOfFour()
        {
            // Arrange
            var secret = "TESTSECRET123456";

            // Act
            var formatted = _twoFactorService.FormatSecretForDisplay(secret);

            // Assert
            Assert.Equal("TEST SECR ET12 3456", formatted);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ValidateTotp_WithEmptySecret_ShouldReturnFalse(string? secret)
        {
            // Act
            var result = _twoFactorService.ValidateTotp(secret!, "123456");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ValidateTotp_WithEmptyCode_ShouldReturnFalse(string? code)
        {
            // Arrange
            var secret = _twoFactorService.GenerateSecret();

            // Act
            var result = _twoFactorService.ValidateTotp(secret, code!);

            // Assert
            Assert.False(result);
        }
    }
}