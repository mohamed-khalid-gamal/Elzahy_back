using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Elzahy.DTOs;
using Elzahy.Services;

namespace Elzahy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("requests")]
        public async Task<ActionResult<ApiResponse<List<AdminRequestResponseDto>>>> GetAdminRequests()
        {
            var result = await _authService.GetAdminRequestsAsync();
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("requests/{requestId}/process")]
        public async Task<ActionResult<ApiResponse<bool>>> ProcessAdminRequest(
            Guid requestId, 
            [FromBody] AdminRequestApprovalDto approval)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId))
                return Unauthorized();

            var result = await _authService.ProcessAdminRequestAsync(adminId, requestId, approval);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserManagementDto>>>> GetAllUsers()
        {
            var result = await _authService.GetAllUsersAsync();
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpDelete("users/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid userId)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId))
                return Unauthorized();

            var result = await _authService.DeleteUserAsync(adminId, userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("users")]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] AdminRegisterRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId))
                return Unauthorized();

            var result = await _authService.CreateUserByAdminAsync(adminId, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}