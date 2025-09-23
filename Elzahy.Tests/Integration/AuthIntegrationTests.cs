using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;
using Elzahy.Services;

namespace Elzahy.Tests.Integration
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryTestDb");
                    });

                    // Ensure the database is created
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CompleteAuthFlow_WithoutTwoFactor_ShouldWork()
        {
            // 1. Register a new user
            var registerRequest = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                Name = "Test User",
                Terms = true
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(registerResult!.Ok);
            Assert.NotNull(registerResult.Data);

            // 2. Login without 2FA
            var loginRequest = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(loginResult!.Ok);
            Assert.False(loginResult.Data!.RequiresTwoFactor);
            Assert.NotEmpty(loginResult.Data.AccessToken);

            // 3. Access protected endpoint
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data.AccessToken);

            var meResponse = await _client.GetAsync("/api/auth/me");
            meResponse.EnsureSuccessStatusCode();

            var meResult = await meResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            Assert.True(meResult!.Ok);
            Assert.Equal("test@example.com", meResult.Data!.Email);
        }

        [Fact]
        public async Task TwoFactorFlow_Setup_Enable_Login_ShouldWork()
        {
            // Setup: Register and login to get access token
            await RegisterAndLogin();
            var accessToken = await GetAccessTokenAsync();

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // 1. Setup 2FA
            var setupResponse = await _client.PostAsync("/api/auth/2fa/setup", null);
            setupResponse.EnsureSuccessStatusCode();

            var setupResult = await setupResponse.Content.ReadFromJsonAsync<ApiResponse<Setup2FAResponseDto>>();
            Assert.True(setupResult!.Ok);
            Assert.NotNull(setupResult.Data);
            Assert.NotEmpty(setupResult.Data.SecretKey);
            Assert.Contains("data:image/png;base64,", setupResult.Data.QrCodeImage);

            // 2. Generate TOTP code (simulated)
            var secret = setupResult.Data.SecretKey;
            using var scope = _factory.Services.CreateScope();
            var twoFactorService = scope.ServiceProvider.GetRequiredService<ITwoFactorService>();
            var totpCode = twoFactorService.GenerateTotp(secret);

            // 3. Enable 2FA
            var enableRequest = new Enable2FARequestDto { Code = totpCode };
            var enableResponse = await _client.PostAsJsonAsync("/api/auth/2fa/enable", enableRequest);
            enableResponse.EnsureSuccessStatusCode();

            var enableResult = await enableResponse.Content.ReadFromJsonAsync<ApiResponse<Enable2FAResponseDto>>();
            Assert.True(enableResult!.Ok);
            Assert.True(enableResult.Data!.Success);
            Assert.NotEmpty(enableResult.Data.RecoveryCodes);

            // 4. Logout and login again (should require 2FA)
            var loginRequest = new LoginRequestDto
            {
                Email = "test2fa@example.com",
                Password = "Password123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(loginResult!.Ok);
            Assert.True(loginResult.Data!.RequiresTwoFactor);
            Assert.NotEmpty(loginResult.Data.TempToken ?? "");

            // 5. Verify 2FA
            var newTotpCode = twoFactorService.GenerateTotp(secret);
            var verifyRequest = new TempTokenVerifyRequestDto
            {
                TempToken = loginResult.Data.TempToken ?? "",
                Code = newTotpCode
            };

            var verifyResponse = await _client.PostAsJsonAsync("/api/auth/2fa/verify", verifyRequest);
            verifyResponse.EnsureSuccessStatusCode();

            var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(verifyResult!.Ok);
            Assert.NotEmpty(verifyResult.Data!.AccessToken);
            Assert.NotEmpty(verifyResult.Data.RefreshToken);
        }

        [Fact]
        public async Task RecoveryCodeFlow_ShouldWork()
        {
            // Setup: Enable 2FA and get recovery codes
            await RegisterAndLogin("recovery@example.com");
            var accessToken = await GetAccessTokenAsync("recovery@example.com");
            var recoveryCodes = await Setup2FAAndGetRecoveryCodes(accessToken);

            // 1. Login (should require 2FA)
            var loginRequest = new LoginRequestDto
            {
                Email = "recovery@example.com",
                Password = "Password123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(loginResult!.Data!.RequiresTwoFactor);

            // 2. Use recovery code instead of TOTP
            var recoveryRequest = new RecoveryCodeVerifyRequestDto
            {
                TempToken = loginResult.Data.TempToken ?? "",
                RecoveryCode = recoveryCodes.First()
            };

            var recoveryResponse = await _client.PostAsJsonAsync("/api/auth/2fa/verify-recovery", recoveryRequest);
            recoveryResponse.EnsureSuccessStatusCode();

            var recoveryResult = await recoveryResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            Assert.True(recoveryResult!.Ok);
            Assert.NotEmpty(recoveryResult.Data!.AccessToken);

            // 3. Try to use the same recovery code again (should fail)
            var loginRequest2 = new LoginRequestDto
            {
                Email = "recovery@example.com",
                Password = "Password123!"
            };

            var loginResponse2 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest2);
            var loginResult2 = await loginResponse2.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();

            var recoveryRequest2 = new RecoveryCodeVerifyRequestDto
            {
                TempToken = loginResult2!.Data!.TempToken ?? "",
                RecoveryCode = recoveryCodes.First() // Same code
            };

            var recoveryResponse2 = await _client.PostAsJsonAsync("/api/auth/2fa/verify-recovery", recoveryRequest2);
            Assert.False(recoveryResponse2.IsSuccessStatusCode);
        }

        private async Task RegisterAndLogin(string email = "test2fa@example.com")
        {
            var registerRequest = new RegisterRequestDto
            {
                Email = email,
                Password = "Password123!",
                Name = "Test User",
                Terms = true
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        }

        private async Task<string> GetAccessTokenAsync(string email = "test2fa@example.com")
        {
            var loginRequest = new LoginRequestDto
            {
                Email = email,
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            return result!.Data!.AccessToken;
        }

        private async Task<List<string>> Setup2FAAndGetRecoveryCodes(string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Setup 2FA
            var setupResponse = await _client.PostAsync("/api/auth/2fa/setup", null);
            var setupResult = await setupResponse.Content.ReadFromJsonAsync<ApiResponse<Setup2FAResponseDto>>();

            // Generate and enable with TOTP
            using var scope = _factory.Services.CreateScope();
            var twoFactorService = scope.ServiceProvider.GetRequiredService<ITwoFactorService>();
            var totpCode = twoFactorService.GenerateTotp(setupResult!.Data!.SecretKey);

            var enableRequest = new Enable2FARequestDto { Code = totpCode };
            var enableResponse = await _client.PostAsJsonAsync("/api/auth/2fa/enable", enableRequest);
            var enableResult = await enableResponse.Content.ReadFromJsonAsync<ApiResponse<Enable2FAResponseDto>>();

            return enableResult!.Data!.RecoveryCodes;
        }
    }
}