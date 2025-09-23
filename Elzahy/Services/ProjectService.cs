using Microsoft.EntityFrameworkCore;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;

namespace Elzahy.Services
{
    public interface IProjectService
    {
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectFormRequestDto request, Guid createdByUserId);
        Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id);
        Task<ApiResponse<List<ProjectDto>>> GetProjectsAsync(ProjectStatus? status = null, bool? isPublished = null);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectFormRequestDto request);
        Task<ApiResponse<bool>> DeleteProjectAsync(Guid id);
        Task<ApiResponse<List<ProjectDto>>> GetProjectsByStatusAsync(ProjectStatus status);
        Task<ApiResponse<ProjectImageDto>> GetProjectImageAsync(Guid imageId);
        Task<ApiResponse<bool>> DeleteProjectImageAsync(Guid imageId);
        Task<ApiResponse<ProjectImageDto>> AddProjectImageAsync(Guid projectId, IFormFile image, string? description, bool isMainImage, Guid createdByUserId);
        Task<ApiResponse<bool>> SetMainImageAsync(Guid projectId, Guid imageId);
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectService> _logger;
        private readonly List<string> _allowedImageTypes = new() { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
        private const int MaxImagesPerProject = 10;

        public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectFormRequestDto request, Guid createdByUserId)
        {
            try
            {
                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    Status = request.Status,
                    TechnologiesUsed = request.TechnologiesUsed,
                    ProjectUrl = request.ProjectUrl,
                    GitHubUrl = request.GitHubUrl,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Client = request.Client,
                    Budget = request.Budget,
                    IsPublished = request.IsPublished,
                    SortOrder = request.SortOrder,
                    CreatedByUserId = createdByUserId
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Handle multiple images upload
                if (request.Images?.Any() == true)
                {
                    if (request.Images.Count > MaxImagesPerProject)
                    {
                        return ApiResponse<ProjectDto>.Failure($"Maximum {MaxImagesPerProject} images allowed per project", 4001);
                    }

                    var images = new List<ProjectImage>();
                    for (int i = 0; i < request.Images.Count; i++)
                    {
                        var imageFile = request.Images[i];
                        var imageResult = await ProcessImageAsync(imageFile);
                        if (!imageResult.Ok)
                            return ApiResponse<ProjectDto>.Failure(imageResult.Error!.Message, imageResult.Error.InternalCode);

                        var projectImage = new ProjectImage
                        {
                            ProjectId = project.Id,
                            ImageData = imageResult.Data!.ImageData,
                            ContentType = imageResult.Data.ContentType,
                            FileName = imageResult.Data.FileName,
                            IsMainImage = request.MainImageIndex.HasValue && request.MainImageIndex.Value == i,
                            SortOrder = i,
                            CreatedByUserId = createdByUserId
                        };

                        images.Add(projectImage);
                    }

                    // Ensure at least one main image if images exist
                    if (images.Any() && !images.Any(i => i.IsMainImage))
                    {
                        images.First().IsMainImage = true;
                    }

                    _context.ProjectImages.AddRange(images);
                    await _context.SaveChangesAsync();
                }

                // Load the project with navigation properties
                var createdProject = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images)
                        .ThenInclude(i => i.CreatedBy)
                    .FirstOrDefaultAsync(p => p.Id == project.Id);

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(createdProject!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create project");
                return ApiResponse<ProjectDto>.Failure($"Failed to create project: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images)
                        .ThenInclude(i => i.CreatedBy)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return ApiResponse<ProjectDto>.Failure("Project not found", 4004);
                }

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project {ProjectId}", id);
                return ApiResponse<ProjectDto>.Failure($"Failed to get project: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsAsync(ProjectStatus? status = null, bool? isPublished = null)
        {
            try
            {
                var query = _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images)
                        .ThenInclude(i => i.CreatedBy)
                    .AsQueryable();

                if (status.HasValue)
                    query = query.Where(p => p.Status == status.Value);

                if (isPublished.HasValue)
                    query = query.Where(p => p.IsPublished == isPublished.Value);

                var projects = await query
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var projectDtos = projects.Select(ProjectDto.FromProject).ToList();
                return ApiResponse<List<ProjectDto>>.Success(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get projects");
                return ApiResponse<List<ProjectDto>>.Failure($"Failed to get projects: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectFormRequestDto request)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images)
                        .ThenInclude(i => i.CreatedBy)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return ApiResponse<ProjectDto>.Failure("Project not found", 4004);
                }

                // Update basic properties
                if (!string.IsNullOrEmpty(request.Name))
                    project.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    project.Description = request.Description;

                if (request.Status.HasValue)
                    project.Status = request.Status.Value;

                if (request.TechnologiesUsed != null)
                    project.TechnologiesUsed = request.TechnologiesUsed;

                if (request.ProjectUrl != null)
                    project.ProjectUrl = request.ProjectUrl;

                if (request.GitHubUrl != null)
                    project.GitHubUrl = request.GitHubUrl;

                if (request.StartDate.HasValue)
                    project.StartDate = request.StartDate;

                if (request.EndDate.HasValue)
                    project.EndDate = request.EndDate;

                if (request.Client != null)
                    project.Client = request.Client;

                if (request.Budget.HasValue)
                    project.Budget = request.Budget;

                if (request.IsPublished.HasValue)
                    project.IsPublished = request.IsPublished.Value;

                if (request.SortOrder.HasValue)
                    project.SortOrder = request.SortOrder.Value;

                // Handle image removals
                if (request.RemoveImageIds?.Any() == true)
                {
                    var imagesToRemove = project.Images
                        .Where(i => request.RemoveImageIds.Contains(i.Id))
                        .ToList();

                    _context.ProjectImages.RemoveRange(imagesToRemove);
                }

                // Handle new images
                if (request.NewImages?.Any() == true)
                {
                    var currentImageCount = project.Images.Count - (request.RemoveImageIds?.Count ?? 0);
                    if (currentImageCount + request.NewImages.Count > MaxImagesPerProject)
                    {
                        return ApiResponse<ProjectDto>.Failure($"Maximum {MaxImagesPerProject} images allowed per project", 4001);
                    }

                    var maxSortOrder = project.Images.Any() ? project.Images.Max(i => i.SortOrder) : -1;
                    
                    foreach (var imageFile in request.NewImages)
                    {
                        var imageResult = await ProcessImageAsync(imageFile);
                        if (!imageResult.Ok)
                            return ApiResponse<ProjectDto>.Failure(imageResult.Error!.Message, imageResult.Error.InternalCode);

                        var projectImage = new ProjectImage
                        {
                            ProjectId = project.Id,
                            ImageData = imageResult.Data!.ImageData,
                            ContentType = imageResult.Data.ContentType,
                            FileName = imageResult.Data.FileName,
                            IsMainImage = false,
                            SortOrder = ++maxSortOrder
                        };

                        _context.ProjectImages.Add(projectImage);
                    }
                }

                // Handle main image change
                if (request.MainImageId.HasValue)
                {
                    foreach (var image in project.Images)
                    {
                        image.IsMainImage = image.Id == request.MainImageId.Value;
                    }
                }

                project.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Reload project with updated images
                await _context.Entry(project)
                    .Collection(p => p.Images)
                    .LoadAsync();

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update project {ProjectId}", id);
                return ApiResponse<ProjectDto>.Failure($"Failed to update project: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (project == null)
                {
                    return ApiResponse<bool>.Failure("Project not found", 4004);
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete project {ProjectId}", id);
                return ApiResponse<bool>.Failure($"Failed to delete project: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsByStatusAsync(ProjectStatus status)
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images)
                        .ThenInclude(i => i.CreatedBy)
                    .Where(p => p.Status == status && p.IsPublished)
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var projectDtos = projects.Select(ProjectDto.FromProject).ToList();
                return ApiResponse<List<ProjectDto>>.Success(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get projects by status {Status}", status);
                return ApiResponse<List<ProjectDto>>.Failure($"Failed to get projects by status: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ProjectImageDto>> GetProjectImageAsync(Guid imageId)
        {
            try
            {
                var image = await _context.ProjectImages
                    .Include(i => i.CreatedBy)
                    .FirstOrDefaultAsync(i => i.Id == imageId);

                if (image == null)
                {
                    return ApiResponse<ProjectImageDto>.Failure("Project image not found", 4004);
                }

                return ApiResponse<ProjectImageDto>.Success(ProjectImageDto.FromProjectImage(image));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project image {ImageId}", imageId);
                return ApiResponse<ProjectImageDto>.Failure($"Failed to get project image: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectImageAsync(Guid imageId)
        {
            try
            {
                var image = await _context.ProjectImages.FirstOrDefaultAsync(i => i.Id == imageId);
                if (image == null)
                {
                    return ApiResponse<bool>.Failure("Project image not found", 4004);
                }

                _context.ProjectImages.Remove(image);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete project image {ImageId}", imageId);
                return ApiResponse<bool>.Failure($"Failed to delete project image: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ProjectImageDto>> AddProjectImageAsync(Guid projectId, IFormFile image, string? description, bool isMainImage, Guid createdByUserId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                {
                    return ApiResponse<ProjectImageDto>.Failure("Project not found", 4004);
                }

                if (project.Images.Count >= MaxImagesPerProject)
                {
                    return ApiResponse<ProjectImageDto>.Failure($"Maximum {MaxImagesPerProject} images allowed per project", 4001);
                }

                var imageResult = await ProcessImageAsync(image);
                if (!imageResult.Ok)
                    return ApiResponse<ProjectImageDto>.Failure(imageResult.Error!.Message, imageResult.Error.InternalCode);

                // If setting as main image, unset other main images
                if (isMainImage)
                {
                    foreach (var existingImage in project.Images)
                    {
                        existingImage.IsMainImage = false;
                    }
                }

                var maxSortOrder = project.Images.Any() ? project.Images.Max(i => i.SortOrder) : -1;

                var projectImage = new ProjectImage
                {
                    ProjectId = projectId,
                    ImageData = imageResult.Data!.ImageData,
                    ContentType = imageResult.Data.ContentType,
                    FileName = imageResult.Data.FileName,
                    Description = description,
                    IsMainImage = isMainImage,
                    SortOrder = maxSortOrder + 1,
                    CreatedByUserId = createdByUserId
                };

                _context.ProjectImages.Add(projectImage);
                await _context.SaveChangesAsync();

                // Load with navigation properties
                await _context.Entry(projectImage)
                    .Reference(i => i.CreatedBy)
                    .LoadAsync();

                return ApiResponse<ProjectImageDto>.Success(ProjectImageDto.FromProjectImage(projectImage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add project image to project {ProjectId}", projectId);
                return ApiResponse<ProjectImageDto>.Failure($"Failed to add project image: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> SetMainImageAsync(Guid projectId, Guid imageId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                {
                    return ApiResponse<bool>.Failure("Project not found", 4004);
                }

                var targetImage = project.Images.FirstOrDefault(i => i.Id == imageId);
                if (targetImage == null)
                {
                    return ApiResponse<bool>.Failure("Image not found in project", 4004);
                }

                // Unset all main images
                foreach (var image in project.Images)
                {
                    image.IsMainImage = false;
                }

                // Set the target as main
                targetImage.IsMainImage = true;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set main image for project {ProjectId}", projectId);
                return ApiResponse<bool>.Failure($"Failed to set main image: {ex.Message}", 5000);
            }
        }

        private async Task<ApiResponse<AwardImageDto>> ProcessImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validate file size
                if (imageFile.Length > MaxImageSize)
                {
                    return ApiResponse<AwardImageDto>.Failure("Image file size cannot exceed 5MB", 4001);
                }

                // Validate content type
                if (!_allowedImageTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    return ApiResponse<AwardImageDto>.Failure("Invalid image format. Allowed formats: JPEG, PNG, GIF, WebP", 4002);
                }

                // Read file data
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                var result = new AwardImageDto
                {
                    ImageData = imageData,
                    ContentType = imageFile.ContentType,
                    FileName = imageFile.FileName
                };

                return ApiResponse<AwardImageDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process image file");
                return ApiResponse<AwardImageDto>.Failure($"Failed to process image: {ex.Message}", 5000);
            }
        }
    }
}