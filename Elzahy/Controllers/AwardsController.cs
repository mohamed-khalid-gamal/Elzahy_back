using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Elzahy.DTOs;
using Elzahy.Services;

namespace Elzahy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardsController : ControllerBase
    {
        private readonly IAwardService _awardService;

        public AwardsController(IAwardService awardService)
        {
            _awardService = awardService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AwardDto>>>> GetAwards([FromQuery] bool? isPublished = null)
        {
            var result = await _awardService.GetAwardsAsync(isPublished);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AwardDto>>> GetAward(Guid id)
        {
            var result = await _awardService.GetAwardAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetAwardImage(Guid id)
        {
            var result = await _awardService.GetAwardImageAsync(id);
            
            if (!result.Ok)
                return NotFound(result);
                
            return File(result.Data!.ImageData, result.Data.ContentType, result.Data.FileName);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<AwardDto>>> CreateAward([FromForm] CreateAwardFormRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _awardService.CreateAwardAsync(request, userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return CreatedAtAction(nameof(GetAward), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<AwardDto>>> UpdateAward(Guid id, [FromForm] UpdateAwardFormRequestDto request)
        {
            var result = await _awardService.UpdateAwardAsync(id, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAward(Guid id)
        {
            var result = await _awardService.DeleteAwardAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}