using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Elzahy.DTOs;
using Elzahy.Models;
using Elzahy.Services;
using System.Text.Json;

namespace Elzahy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetProjects(
            [FromQuery] ProjectStatus? status = null,
            [FromQuery] bool? isPublished = null)
        {
            var result = await _projectService.GetProjectsAsync(status, isPublished);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
        {
            var result = await _projectService.GetProjectAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetProjectsByStatus(ProjectStatus status)
        {
            var result = await _projectService.GetProjectsByStatusAsync(status);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromForm] CreateProjectFormRequestDto request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _projectService.CreateProjectAsync(request, userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromForm] UpdateProjectFormRequestDto request)
        {
            var result = await _projectService.UpdateProjectAsync(id, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(Guid id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        // Image management endpoints
        [HttpGet("images/{imageId}")]
        public async Task<IActionResult> GetProjectImage(Guid imageId)
        {
            var result = await _projectService.GetProjectImageAsync(imageId);
            
            if (!result.Ok || result.Data == null)
                return NotFound();
                
            var imageBytes = Convert.FromBase64String(result.Data.ImageData);
            return File(imageBytes, result.Data.ContentType, result.Data.FileName);
        }

        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectImageDto>>> AddProjectImage(
            Guid id, 
            [FromForm] IFormFile image, 
            [FromForm] string? description = null, 
            [FromForm] bool isMainImage = false)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _projectService.AddProjectImageAsync(id, image, description, isMainImage, userId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpDelete("images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProjectImage(Guid imageId)
        {
            var result = await _projectService.DeleteProjectImageAsync(imageId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPut("{projectId}/images/{imageId}/set-main")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetMainImage(Guid projectId, Guid imageId)
        {
            var result = await _projectService.SetMainImageAsync(projectId, imageId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}