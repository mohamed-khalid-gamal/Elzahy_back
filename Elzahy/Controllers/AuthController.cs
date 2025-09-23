using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Elzahy.DTOs;
using Elzahy.Services;

namespace Elzahy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/verify")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyTwoFactor([FromBody] TempTokenVerifyRequestDto request)
        {
            var result = await _authService.VerifyTwoFactorAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/verify-recovery")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyRecoveryCode([FromBody] RecoveryCodeVerifyRequestDto request)
        {
            var result = await _authService.VerifyRecoveryCodeAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenRefreshResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.GetUserAsync(userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateUserRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.UpdateUserAsync(userId, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/setup")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Setup2FAResponseDto>>> Setup2FA()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.Setup2FAAsync(userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/enable")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Enable2FAResponseDto>>> Enable2FA([FromBody] Enable2FARequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.Enable2FAAsync(userId, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/disable")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Disable2FA()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.Disable2FAAsync(userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("2fa/recovery-codes")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RecoveryCodesResponseDto>>> GenerateRecoveryCodes()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.GenerateNewRecoveryCodesAsync(userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmEmail([FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(token);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("request-admin")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<AdminRequestResponseDto>>> RequestAdminRole([FromBody] AdminRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _authService.RequestAdminRoleAsync(userId, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}