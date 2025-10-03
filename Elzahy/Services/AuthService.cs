using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Security.Cryptography;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;
using BCrypt.Net;

namespace Elzahy.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<TokenRefreshResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<UserDto>> GetUserAsync(Guid userId);
        Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequestDto request);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
        Task<ApiResponse<AdminRequestResponseDto>> RequestAdminRoleAsync(Guid userId, AdminRequestDto request);
        Task<ApiResponse<List<AdminRequestResponseDto>>> GetAdminRequestsAsync();
        Task<ApiResponse<bool>> ProcessAdminRequestAsync(Guid adminId, Guid requestId, AdminRequestApprovalDto approval);
        Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync();
        Task<ApiResponse<bool>> DeleteUserAsync(Guid adminId, Guid userIdToDelete);
        Task<ApiResponse<UserDto>> CreateUserByAdminAsync(Guid adminId, AdminRegisterRequestDto request);
        Task<ApiResponse<Setup2FAResponseDto>> Setup2FAAsync(Guid userId);
        Task<ApiResponse<Enable2FAResponseDto>> Enable2FAAsync(Guid userId, Enable2FARequestDto request);
        Task<ApiResponse<bool>> Disable2FAAsync(Guid userId);
        Task<ApiResponse<AuthResponseDto>> VerifyTwoFactorAsync(TempTokenVerifyRequestDto request);
        Task<ApiResponse<AuthResponseDto>> VerifyRecoveryCodeAsync(RecoveryCodeVerifyRequestDto request);
        Task<ApiResponse<RecoveryCodesResponseDto>> GenerateNewRecoveryCodesAsync(Guid userId);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<ApiResponse<bool>> ConfirmEmailAsync(string token);
        Task LogoutAsync(string refreshToken);
        Task SeedDefaultAdminAsync();
    }
  
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _tempTokenProtector;
        private readonly ILogger<AuthService> _logger;
        
        public AuthService(
            AppDbContext context, 
            IJwtService jwtService, 
            ITwoFactorService twoFactorService,
            IEmailService emailService,
            IConfiguration configuration,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _twoFactorService = twoFactorService;
            _emailService = emailService;
            _configuration = configuration;
            _tempTokenProtector = dataProtectionProvider.CreateProtector("TempTokens");
            _logger = logger;
        }
        
        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if terms are accepted
                if (!request.Terms)
                {
                    return ApiResponse<AuthResponseDto>.Failure("You must accept the terms and conditions", 4002);
                }
                
                // Normalize email
                var email = request.Email.ToLowerInvariant();
                
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("User with this email already exists", 4003);
                }
                
                // Determine if this should be the first admin (no existing admins)
                var hasAnyAdmin = await _context.Users.AnyAsync(u => u.Role == "Admin");
                var assignAdminRole = !hasAnyAdmin; // first ever user becomes Admin automatically
                if (assignAdminRole)
                {
                    _logger.LogInformation("No admins found. First registered user {Email} will be granted Admin role automatically.", email);
                }
                
                // Create new user
                var user = new User
                {
                    Email = email,
                    Name = request.Name,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Language = "en-US",
                    EmailConfirmationToken = Guid.NewGuid().ToString(),
                    EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
                    Role = assignAdminRole ? "Admin" : "User"
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // If user requested admin role and we didn't auto-assign admin (i.e., admins already exist), create admin request
                if (request.RequestAdminRole && !assignAdminRole)
                {
                    var adminRequest = new AdminRequest
                    {
                        UserId = user.Id,
                        Reason = "Admin role requested during registration",
                        AdditionalInfo = $"User {user.Name} ({user.Email}) requested admin privileges during account creation."
                    };
                    
                    _context.AdminRequests.Add(adminRequest);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Admin role request created for user {Email}", email);
                }

                // Send email confirmation
                var confirmationLink = $"{_configuration["App:BaseUrl"]}/api/auth/confirm-email?token={user.EmailConfirmationToken}";
                await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);
                
                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
                
                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} registered successfully with role {Role}", email, user.Role);
                
                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserDto.FromUser(user),
                    ExpiresIn = expiresInMinutes * 60 // Convert to seconds
                };
                
                return ApiResponse<AuthResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", request.Email);
                return ApiResponse<AuthResponseDto>.Failure($"Registration failed: {ex.Message}", 5000);
            }
        }
        
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    await RecordFailedLoginAttempt(email);
                    return ApiResponse<AuthResponseDto>.Failure("Invalid credentials", 4001);
                }

                // Check if account is locked
                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    var lockoutMinutes = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                    return ApiResponse<AuthResponseDto>.Failure($"Account is locked. Try again in {Math.Ceiling(lockoutMinutes)} minutes.", 4029);
                }

                // Reset failed login attempts on successful login
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;

                // Check if 2FA is enabled
                if (user.TwoFactorEnabled)
                {
                    // Generate temp token
                    var tempToken = GenerateTempToken(user.Id);
                    var tempTokenExpiry = int.Parse(_configuration["JwtSettings:TempTokenExpirationInMinutes"] ?? "5");
                    
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("2FA required for user {Email}", email);
                    
                    return ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
                    {
                        RequiresTwoFactor = true,
                        TempToken = tempToken,
                        AccessToken = "",
                        RefreshToken = "",
                        User = null,
                        ExpiresIn = tempTokenExpiry * 60 // Convert to seconds
                    });
                }

                // Generate tokens for non-2FA users
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
                
                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} logged in successfully", email);
                
                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserDto.FromUser(user),
                    ExpiresIn = expiresInMinutes * 60
                };
                
                return ApiResponse<AuthResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", request.Email);
                return ApiResponse<AuthResponseDto>.Failure($"Login failed: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> VerifyTwoFactorAsync(TempTokenVerifyRequestDto request)
        {
            try
            {
                // Validate and decode temp token
                var userId = ValidateTempToken(request.TempToken);
                if (userId == Guid.Empty)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Invalid or expired temporary token", 4001);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("User not found", 4004);
                }

                // Validate TOTP code
                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    return ApiResponse<AuthResponseDto>.Failure("2FA not properly configured", 4005);
                }

                var isValidTotp = _twoFactorService.ValidateTotp(user.TwoFactorSecret, request.Code);
                if (!isValidTotp)
                {
                    await RecordFailedLoginAttempt(user.Email);
                    return ApiResponse<AuthResponseDto>.Failure("Invalid 2FA code", 4001);
                }

                // Generate final tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
                
                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(refreshTokenEntity);
                
                // Reset failed login attempts
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} completed 2FA verification", user.Email);
                
                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserDto.FromUser(user),
                    ExpiresIn = expiresInMinutes * 60
                };
                
                return ApiResponse<AuthResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA verification failed");
                return ApiResponse<AuthResponseDto>.Failure($"2FA verification failed: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> VerifyRecoveryCodeAsync(RecoveryCodeVerifyRequestDto request)
        {
            try
            {
                // Validate and decode temp token
                var userId = ValidateTempToken(request.TempToken);
                if (userId == Guid.Empty)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Invalid or expired temporary token", 4001);
                }

                var user = await _context.Users
                    .Include(u => u.RecoveryCodes.Where(rc => !rc.IsUsed))
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("User not found", 4004);
                }

                // Find matching recovery code
                var matchingCode = user.RecoveryCodes.FirstOrDefault(rc => 
                    !rc.IsUsed && BCrypt.Net.BCrypt.Verify(request.RecoveryCode, rc.CodeHash));

                if (matchingCode == null)
                {
                    await RecordFailedLoginAttempt(user.Email);
                    return ApiResponse<AuthResponseDto>.Failure("Invalid recovery code", 4001);
                }

                // Mark recovery code as used
                matchingCode.IsUsed = true;
                matchingCode.UsedAt = DateTime.UtcNow;

                // Generate final tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
                
                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(refreshTokenEntity);
                
                // Reset failed login attempts
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} used recovery code for authentication", user.Email);
                
                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserDto.FromUser(user),
                    ExpiresIn = expiresInMinutes * 60
                };
                
                return ApiResponse<AuthResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recovery code verification failed");
                return ApiResponse<AuthResponseDto>.Failure($"Recovery code verification failed: {ex.Message}", 5000);
            }
        }
        
        public async Task<ApiResponse<TokenRefreshResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenEntity = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
                
                if (tokenEntity == null || tokenEntity.ExpiresAt <= DateTime.UtcNow)
                {
                    return ApiResponse<TokenRefreshResponseDto>.Failure("Invalid or expired refresh token", 4001);
                }
                
                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(tokenEntity.User);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
                
                // Revoke old refresh token
                tokenEntity.IsRevoked = true;
                
                // Create new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = tokenEntity.UserId,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token renewed for user {UserId}", tokenEntity.UserId);
                
                var response = new TokenRefreshResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = expiresInMinutes * 60
                };
                
                return ApiResponse<TokenRefreshResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return ApiResponse<TokenRefreshResponseDto>.Failure($"Token refresh failed: {ex.Message}", 5000);
            }
        }
        
        public async Task<ApiResponse<UserDto>> GetUserAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.Failure("User not found", 4004);
                }
                
                return ApiResponse<UserDto>.Success(UserDto.FromUser(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user {UserId}", userId);
                return ApiResponse<UserDto>.Failure($"Failed to get user: {ex.Message}", 5000);
            }
        }
        
        public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.Failure("User not found", 4004);
                }
                
                if (!string.IsNullOrEmpty(request.Name))
                    user.Name = request.Name;
                
                if (!string.IsNullOrEmpty(request.Language) && (request.Language == "en-US" || request.Language == "es-ES"))
                    user.Language = request.Language;
                
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User profile updated for {UserId}", userId);
                
                return ApiResponse<UserDto>.Success(UserDto.FromUser(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", userId);
                return ApiResponse<UserDto>.Failure($"Failed to update user: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.Failure("User not found", 4004);
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return ApiResponse<bool>.Failure("Current password is incorrect", 4001);
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                
                // Reset failed login attempts and lockout
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                
                await _context.SaveChangesAsync();

                // Send notification email about password change
                await _emailService.SendPasswordChangeNotificationAsync(user.Email, user.Name);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change password for user {UserId}", userId);
                return ApiResponse<bool>.Failure($"Failed to change password: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AdminRequestResponseDto>> RequestAdminRoleAsync(Guid userId, AdminRequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<AdminRequestResponseDto>.Failure("User not found", 4004);
                }

                if (user.Role == "Admin")
                {
                    return ApiResponse<AdminRequestResponseDto>.Failure("User is already an admin", 4006);
                }

                // Check if user has pending admin request
                var existingRequest = await _context.AdminRequests
                    .FirstOrDefaultAsync(ar => ar.UserId == userId && !ar.IsProcessed);

                if (existingRequest != null)
                {
                    return ApiResponse<AdminRequestResponseDto>.Failure("You already have a pending admin request", 4007);
                }

                var adminRequest = new AdminRequest
                {
                    UserId = userId,
                    Reason = request.Reason,
                    AdditionalInfo = request.AdditionalInfo
                };

                _context.AdminRequests.Add(adminRequest);
                await _context.SaveChangesAsync();

                // Load user for response
                adminRequest.User = user;

                _logger.LogInformation("Admin request created for user {UserId}", userId);

                return ApiResponse<AdminRequestResponseDto>.Success(AdminRequestResponseDto.FromAdminRequest(adminRequest));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create admin request for user {UserId}", userId);
                return ApiResponse<AdminRequestResponseDto>.Failure($"Failed to create admin request: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<List<AdminRequestResponseDto>>> GetAdminRequestsAsync()
        {
            try
            {
                var requests = await _context.AdminRequests
                    .Include(ar => ar.User)
                    .Include(ar => ar.ProcessedByAdmin)
                    .OrderByDescending(ar => ar.CreatedAt)
                    .ToListAsync();

                var response = requests.Select(AdminRequestResponseDto.FromAdminRequest).ToList();

                return ApiResponse<List<AdminRequestResponseDto>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get admin requests");
                return ApiResponse<List<AdminRequestResponseDto>>.Failure($"Failed to get admin requests: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> ProcessAdminRequestAsync(Guid adminId, Guid requestId, AdminRequestApprovalDto approval)
        {
            try
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == "Admin");
                if (admin == null)
                {
                    return ApiResponse<bool>.Failure("Admin user not found or insufficient permissions", 4008);
                }

                var adminRequest = await _context.AdminRequests
                    .Include(ar => ar.User)
                    .FirstOrDefaultAsync(ar => ar.Id == requestId);

                if (adminRequest == null)
                {
                    return ApiResponse<bool>.Failure("Admin request not found", 4004);
                }

                if (adminRequest.IsProcessed)
                {
                    return ApiResponse<bool>.Failure("Admin request has already been processed", 4009);
                }

                adminRequest.IsProcessed = true;
                adminRequest.IsApproved = approval.Approved;
                adminRequest.AdminNotes = approval.AdminNotes;
                adminRequest.ProcessedByAdminId = adminId;
                adminRequest.ProcessedAt = DateTime.UtcNow;
                adminRequest.UpdatedAt = DateTime.UtcNow;

                if (approval.Approved)
                {
                    adminRequest.User.Role = "Admin";
                    adminRequest.User.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Send notification email
                await _emailService.SendAdminRequestNotificationAsync(
                    adminRequest.User.Email, 
                    adminRequest.User.Name, 
                    approval.Approved,
                    approval.AdminNotes);

                _logger.LogInformation("Admin request {RequestId} processed by {AdminId}. Approved: {Approved}", 
                    requestId, adminId, approval.Approved);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process admin request {RequestId}", requestId);
                return ApiResponse<bool>.Failure($"Failed to process admin request: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.AdminRequests.Where(ar => !ar.IsProcessed))
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                var response = users.Select(u => UserManagementDto.FromUser(u, u.AdminRequests.Any())).ToList();

                return ApiResponse<List<UserManagementDto>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users");
                return ApiResponse<List<UserManagementDto>>.Failure($"Failed to get users: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(Guid adminId, Guid userIdToDelete)
        {
            try
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == "Admin");
                if (admin == null)
                {
                    return ApiResponse<bool>.Failure("Admin user not found or insufficient permissions", 4008);
                }

                var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdToDelete);
                if (userToDelete == null)
                {
                    return ApiResponse<bool>.Failure("User not found", 4004);
                }

                // Prevent admin from deleting themselves
                if (adminId == userIdToDelete)
                {
                    return ApiResponse<bool>.Failure("Admins cannot delete their own account", 4010);
                }

                // Prevent deleting the last admin (if this user is admin)
                if (userToDelete.Role == "Admin")
                {
                    var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                    if (adminCount <= 1)
                    {
                        return ApiResponse<bool>.Failure("Cannot delete the last admin user", 4011);
                    }
                }

                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserIdToDelete} deleted by admin {AdminId}", userIdToDelete, adminId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserIdToDelete}", userIdToDelete);
                return ApiResponse<bool>.Failure($"Failed to delete user: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserByAdminAsync(Guid adminId, AdminRegisterRequestDto request)
        {
            try
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == "Admin");
                if (admin == null)
                {
                    return ApiResponse<UserDto>.Failure("Admin user not found or insufficient permissions", 4008);
                }

                // Normalize email
                var email = request.Email.ToLowerInvariant();
                
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return ApiResponse<UserDto>.Failure("User with this email already exists", 4003);
                }

                // Validate role
                if (request.Role != "User" && request.Role != "Admin")
                {
                    return ApiResponse<UserDto>.Failure("Invalid role. Must be 'User' or 'Admin'", 4012);
                }
                
                // Create new user
                var user = new User
                {
                    Email = email,
                    Name = request.Name,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = request.Role,
                    Language = "en-US",
                    EmailConfirmed = true // Admin-created users are auto-confirmed
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send welcome email with credentials
                await _emailService.SendAdminCreatedUserEmailAsync(user.Email, user.Name, request.Password);

                _logger.LogInformation("User {Email} created by admin {AdminId}", email, adminId);
                
                return ApiResponse<UserDto>.Success(UserDto.FromUser(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user by admin {AdminId}", adminId);
                return ApiResponse<UserDto>.Failure($"Failed to create user: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<Setup2FAResponseDto>> Setup2FAAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<Setup2FAResponseDto>.Failure("User not found", 4004);
                }

                // Generate new secret if not exists
                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    user.TwoFactorSecret = _twoFactorService.GenerateSecret();
                    await _context.SaveChangesAsync();
                }

                var issuer = _configuration["TwoFactor:Issuer"] ?? "Elzahy Portfolio";
                var qrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Email, user.TwoFactorSecret, issuer);
                var qrCodeImage = _twoFactorService.GenerateQrCodeImage(qrCodeUrl);
                var qrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
                var manualKey = _twoFactorService.FormatSecretForDisplay(user.TwoFactorSecret);

                _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

                var response = new Setup2FAResponseDto
                {
                    SecretKey = user.TwoFactorSecret,
                    QrCodeImage = qrCodeBase64,
                    ManualEntryKey = manualKey
                };

                return ApiResponse<Setup2FAResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to setup 2FA for user {UserId}", userId);
                return ApiResponse<Setup2FAResponseDto>.Failure($"Failed to setup 2FA: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<Enable2FAResponseDto>> Enable2FAAsync(Guid userId, Enable2FARequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResponse<Enable2FAResponseDto>.Failure("User not found", 4004);
                }

                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    return ApiResponse<Enable2FAResponseDto>.Failure("2FA setup required first", 4005);
                }

                var isValid = _twoFactorService.ValidateTotp(user.TwoFactorSecret, request.Code);
                if (!isValid)
                {
                    return ApiResponse<Enable2FAResponseDto>.Failure("Invalid verification code", 4001);
                }

                user.TwoFactorEnabled = true;

                // Generate recovery codes
                var recoveryCodes = await GenerateRecoveryCodesForUser(userId, 10);
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("2FA enabled for user {UserId}", userId);

                var response = new Enable2FAResponseDto
                {
                    Success = true,
                    RecoveryCodes = recoveryCodes,
                    Message = "Two-factor authentication has been enabled successfully."
                };

                return ApiResponse<Enable2FAResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enable 2FA for user {UserId}", userId);
                return ApiResponse<Enable2FAResponseDto>.Failure($"Failed to enable 2FA: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> Disable2FAAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.RecoveryCodes)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return ApiResponse<bool>.Failure("User not found", 4004);
                }

                user.TwoFactorEnabled = false;
                user.TwoFactorSecret = null;

                // Remove all recovery codes
                _context.RecoveryCodes.RemoveRange(user.RecoveryCodes);
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("2FA disabled for user {UserId}", userId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disable 2FA for user {UserId}", userId);
                return ApiResponse<bool>.Failure($"Failed to disable 2FA: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<RecoveryCodesResponseDto>> GenerateNewRecoveryCodesAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.RecoveryCodes)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return ApiResponse<RecoveryCodesResponseDto>.Failure("User not found", 4004);
                }

                if (!user.TwoFactorEnabled)
                {
                    return ApiResponse<RecoveryCodesResponseDto>.Failure("2FA must be enabled to generate recovery codes", 4005);
                }

                // Remove existing recovery codes
                _context.RecoveryCodes.RemoveRange(user.RecoveryCodes);

                // Generate new recovery codes
                var recoveryCodes = await GenerateRecoveryCodesForUser(userId, 10);
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("New recovery codes generated for user {UserId}", userId);

                var response = new RecoveryCodesResponseDto
                {
                    RecoveryCodes = recoveryCodes,
                    Count = recoveryCodes.Count
                };

                return ApiResponse<RecoveryCodesResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate recovery codes for user {UserId}", userId);
                return ApiResponse<RecoveryCodesResponseDto>.Failure($"Failed to generate recovery codes: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
                if (user == null)
                {
                    // Don't reveal if email exists
                    return ApiResponse<bool>.Success(true);
                }

                user.PasswordResetToken = Guid.NewGuid().ToString();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = $"{_configuration["App:FrontendUrl"]}/reset-password?token={user.PasswordResetToken}";
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

                _logger.LogInformation("Password reset requested for {Email}", request.Email);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process password reset for {Email}", request.Email);
                return ApiResponse<bool>.Failure($"Failed to process password reset: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.PasswordResetToken == request.Token && 
                    u.PasswordResetTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    return ApiResponse<bool>.Failure("Invalid or expired reset token", 4001);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                
                // Reset failed login attempts and lockout
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset completed for user {UserId}", user.Id);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password");
                return ApiResponse<bool>.Failure($"Failed to reset password: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> ConfirmEmailAsync(string token)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.EmailConfirmationToken == token && 
                    u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    return ApiResponse<bool>.Failure("Invalid or expired confirmation token", 4001);
                }

                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);

                _logger.LogInformation("Email confirmed for user {UserId}", user.Id);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to confirm email");
                return ApiResponse<bool>.Failure($"Failed to confirm email: {ex.Message}", 5000);
            }
        }
        
        public async Task LogoutAsync(string refreshToken)
        {
            try
            {
                var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
                if (tokenEntity != null)
                {
                    tokenEntity.IsRevoked = true;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User logged out, refresh token revoked");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }
        
        public async Task SeedDefaultAdminAsync()
        {
            try
            {
                var adminEmail = "mohamed.khalid.gamal@gmail.com";
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
                
                if (existingAdmin == null)
                {
                    var admin = new User
                    {
                        Email = adminEmail,
                        Name = "Administrator",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin12345"),
                        Role = "Admin",
                        Language = "en-US",
                        EmailConfirmed = true
                    };
                    
                    _context.Users.Add(admin);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Default admin user seeded");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed default admin");
            }
        }

        // Private helper methods
        private string GenerateTempToken(Guid userId)
        {
            var payload = new { UserId = userId, IssuedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(payload);
            return _tempTokenProtector.Protect(json);
        }

        private Guid ValidateTempToken(string tempToken)
        {
            try
            {
                var json = _tempTokenProtector.Unprotect(tempToken);
                var payload = JsonSerializer.Deserialize<dynamic>(json);
                
                var issuedAt = DateTime.Parse(payload.GetProperty("IssuedAt").GetString()!);
                var expirationMinutes = int.Parse(_configuration["JwtSettings:TempTokenExpirationInMinutes"] ?? "5");
                
                if (DateTime.UtcNow > issuedAt.AddMinutes(expirationMinutes))
                {
                    return Guid.Empty; // Token expired
                }
                
                return Guid.Parse(payload.GetProperty("UserId").GetString()!);
            }
            catch
            {
                return Guid.Empty; // Invalid token
            }
        }

        private async Task<List<string>> GenerateRecoveryCodesForUser(Guid userId, int count)
        {
            var recoveryCodes = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var code = GenerateRecoveryCode();
                recoveryCodes.Add(code);
                
                var recoveryCodeEntity = new RecoveryCode
                {
                    UserId = userId,
                    CodeHash = BCrypt.Net.BCrypt.HashPassword(code)
                };
                
                _context.RecoveryCodes.Add(recoveryCodeEntity);
            }
            
            return recoveryCodes;
        }

        private string GenerateRecoveryCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[5];
            rng.GetBytes(bytes);
            
            var code = "";
            foreach (var b in bytes)
            {
                code += (b % 10).ToString();
            }
            
            // Format as XXX-XXXX
            return $"{code.Substring(0, 3)}-{code.Substring(3, 2)}{code.Substring(0, 2)}";
        }

        private async Task RecordFailedLoginAttempt(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    user.FailedLoginAttempts++;
                    
                    // Lock account after 5 failed attempts
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                        _logger.LogWarning("Account locked for user {Email} due to failed login attempts", email);
                    }
                    
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record failed login attempt for {Email}", email);
            }
        }
    }
}