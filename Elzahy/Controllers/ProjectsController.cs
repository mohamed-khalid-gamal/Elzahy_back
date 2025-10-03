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

        /// <summary>
        /// Get projects with advanced filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectDto>>>> GetProjects([FromQuery] ProjectFilterDto filter)
        {
            var result = await _projectService.GetProjectsAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get projects summary (lightweight version for listings)
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectSummaryDto>>>> GetProjectsSummary([FromQuery] ProjectFilterDto filter)
        {
            var result = await _projectService.GetProjectsSummaryAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get featured projects
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<List<ProjectSummaryDto>>>> GetFeaturedProjects(
            [FromQuery] int count = 6,
            [FromQuery] string? language = null)
        {
            var result = await _projectService.GetFeaturedProjectsAsync(count, language);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get projects by status with optional language filter
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectSummaryDto>>>> GetProjectsByStatus(
            ProjectStatus status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? language = null)
        {
            var filter = new ProjectFilterDto
            {
                Status = status,
                IsPublished = true,
                Language = language,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _projectService.GetProjectsSummaryAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Search projects by term
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectSummaryDto>>>> SearchProjects(
            [FromQuery] string searchTerm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? language = null,
            [FromQuery] ProjectStatus? status = null)
        {
            var filter = new ProjectFilterDto
            {
                SearchTerm = searchTerm,
                Status = status,
                IsPublished = true,
                Language = language,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _projectService.GetProjectsSummaryAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get project by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id, [FromQuery] string? language = null)
        {
            var result = await _projectService.GetProjectAsync(id, language);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get projects by property type
        /// </summary>
        [HttpGet("by-property-type/{propertyType}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectSummaryDto>>>> GetProjectsByPropertyType(
            string propertyType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? language = null)
        {
            var filter = new ProjectFilterDto
            {
                PropertyType = propertyType,
                IsPublished = true,
                Language = language,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _projectService.GetProjectsSummaryAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get projects by location
        /// </summary>
        [HttpGet("by-location/{location}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectSummaryDto>>>> GetProjectsByLocation(
            string location,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? language = null)
        {
            var filter = new ProjectFilterDto
            {
                Location = location,
                IsPublished = true,
                Language = language,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _projectService.GetProjectsSummaryAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Create new project (Admin only)
        /// </summary>
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

        /// <summary>
        /// Update project (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromForm] UpdateProjectFormRequestDto request)
        {
            var result = await _projectService.UpdateProjectAsync(id, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Delete project (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(Guid id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Toggle project featured status (Admin only)
        /// </summary>
        [HttpPut("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleFeatured(Guid id)
        {
            var result = await _projectService.ToggleFeaturedAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        // Image management endpoints
        /// <summary>
        /// Get project image by ID (serves the actual image file)
        /// </summary>
        [HttpGet("images/{imageId}")]
        public async Task<IActionResult> GetProjectImage(Guid imageId)
        {
            var streamResult = await _projectService.GetProjectImageStreamAsync(imageId);
            
            if (!streamResult.Ok || streamResult.Data == null)
                return NotFound(streamResult.Error?.Message ?? "Image not found");

            var imageResult = await _projectService.GetProjectImageAsync(imageId);
            if (!imageResult.Ok)
                return NotFound();

            var contentType = imageResult.Data!.ContentType;
            var fileName = imageResult.Data.FileName;

            return File(streamResult.Data, contentType, fileName, enableRangeProcessing: true);
        }

        /// <summary>
        /// Add image to project (Admin only)
        /// </summary>
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

        /// <summary>
        /// Delete project image (Admin only)
        /// </summary>
        [HttpDelete("images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProjectImage(Guid imageId)
        {
            var result = await _projectService.DeleteProjectImageAsync(imageId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Set main image for project (Admin only)
        /// </summary>
        [HttpPut("{projectId}/images/{imageId}/set-main")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetMainImage(Guid projectId, Guid imageId)
        {
            var result = await _projectService.SetMainImageAsync(projectId, imageId);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        // Video management endpoints
        /// <summary>
        /// Get project video by ID (serves the actual video file)
        /// </summary>
        [HttpGet("videos/{videoId}")]
        public async Task<IActionResult> GetProjectVideo(Guid videoId)
        {
            var streamResult = await _projectService.GetProjectVideoStreamAsync(videoId);
            
            if (!streamResult.Ok || streamResult.Data == null)
                return NotFound(streamResult.Error?.Message ?? "Video not found");

            var videoResult = await _projectService.GetProjectVideoAsync(videoId);
            if (!videoResult.Ok)
                return NotFound();

            var contentType = videoResult.Data!.ContentType;
            var fileName = videoResult.Data.FileName;

            return File(streamResult.Data, contentType, fileName, enableRangeProcessing: true);
        }

        /// <summary>
        /// Add video to project (Admin only)
        /// </summary>
        [HttpPost("{id}/videos")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectVideoDto>>> AddProjectVideo(
            Guid id,
            [FromForm] IFormFile video,
            [FromForm] string? description = null,
            [FromForm] bool isMainVideo = false)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _projectService.AddProjectVideoAsync(id, video, description, isMainVideo, userId);
            if (!result.Ok)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete project video (Admin only)
        /// </summary>
        [HttpDelete("videos/{videoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProjectVideo(Guid videoId)
        {
            var result = await _projectService.DeleteProjectVideoAsync(videoId);
            if (!result.Ok)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Set main video for project (Admin only)
        /// </summary>
        [HttpPut("{projectId}/videos/{videoId}/set-main")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetMainVideo(Guid projectId, Guid videoId)
        {
            var result = await _projectService.SetMainVideoAsync(projectId, videoId);
            if (!result.Ok)
                return BadRequest(result);

            return Ok(result);
        }

        // Translation management endpoints
        /// <summary>
        /// Add or update project translation (Admin only)
        /// </summary>
        [HttpPost("{id}/translations")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectTranslationDto>>> UpsertProjectTranslation(
            Guid id,
            [FromBody] ProjectTranslationUpsertDto translation)
        {
            var result = await _projectService.UpsertProjectTranslationAsync(id, translation);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Delete project translation (Admin only)
        /// </summary>
        [HttpDelete("{id}/translations/{language}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProjectTranslation(Guid id, string language)
        {
            var result = await _projectService.DeleteProjectTranslationAsync(id, language);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        // Analytics endpoints (for admin dashboard)
        /// <summary>
        /// Get project statistics (Admin only)
        /// </summary>
        [HttpGet("analytics/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> GetProjectStatistics()
        {
            var result = await _projectService.GetProjectStatisticsAsync();
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}