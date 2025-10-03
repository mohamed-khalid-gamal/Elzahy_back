using Microsoft.EntityFrameworkCore;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;
using System.Linq.Expressions;

namespace Elzahy.Services
{
    public interface IProjectService
    {
        // Enhanced project operations
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectFormRequestDto request, Guid createdByUserId);
        Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id, string? language = null);
        Task<ApiResponse<PagedResponse<ProjectDto>>> GetProjectsAsync(ProjectFilterDto filter);
        Task<ApiResponse<PagedResponse<ProjectSummaryDto>>> GetProjectsSummaryAsync(ProjectFilterDto filter);
        Task<ApiResponse<List<ProjectSummaryDto>>> GetFeaturedProjectsAsync(int count = 6, string? language = null);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectFormRequestDto request);
        Task<ApiResponse<bool>> DeleteProjectAsync(Guid id);
        Task<ApiResponse<bool>> ToggleFeaturedAsync(Guid id);

        // Legacy method for backward compatibility
        Task<ApiResponse<List<ProjectDto>>> GetProjectsByStatusAsync(ProjectStatus status);

        // Images
        Task<ApiResponse<ProjectImageDto>> GetProjectImageAsync(Guid imageId);
        Task<ApiResponse<Stream>> GetProjectImageStreamAsync(Guid imageId);
        Task<ApiResponse<bool>> DeleteProjectImageAsync(Guid imageId);
        Task<ApiResponse<ProjectImageDto>> AddProjectImageAsync(Guid projectId, IFormFile image, string? description, bool isMainImage, Guid createdByUserId);
        Task<ApiResponse<bool>> SetMainImageAsync(Guid projectId, Guid imageId);

        // Videos
        Task<ApiResponse<ProjectVideoDto>> GetProjectVideoAsync(Guid videoId);
        Task<ApiResponse<Stream>> GetProjectVideoStreamAsync(Guid videoId);
        Task<ApiResponse<ProjectVideoDto>> AddProjectVideoAsync(Guid projectId, IFormFile video, string? description, bool isMainVideo, Guid createdByUserId);
        Task<ApiResponse<bool>> DeleteProjectVideoAsync(Guid videoId);
        Task<ApiResponse<bool>> SetMainVideoAsync(Guid projectId, Guid videoId);

        // Translations
        Task<ApiResponse<ProjectTranslationDto>> UpsertProjectTranslationAsync(Guid projectId, ProjectTranslationUpsertDto translation);
        Task<ApiResponse<bool>> DeleteProjectTranslationAsync(Guid projectId, string language);

        // Analytics
        Task<ApiResponse<object>> GetProjectStatisticsAsync();
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectService> _logger;
        private readonly IFileStorageService _fileStorage;

        public ProjectService(AppDbContext context, ILogger<ProjectService> logger, IFileStorageService fileStorage)
        {
            _context = context;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectFormRequestDto request, Guid createdByUserId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    Status = request.Status,
                    
                    // Real Estate Fields
                    CompanyUrl = request.CompanyUrl,
                    GoogleMapsUrl = request.GoogleMapsUrl,
                    Location = request.Location,
                    PropertyType = request.PropertyType,
                    TotalUnits = request.TotalUnits,
                    ProjectArea = request.ProjectArea,
                    PriceStart = request.PriceStart,
                    PriceEnd = request.PriceEnd,
                    PriceCurrency = request.PriceCurrency,
                    
                    // Legacy fields for compatibility
                    TechnologiesUsed = request.TechnologiesUsed,
                    ProjectUrl = request.ProjectUrl,
                    GitHubUrl = request.GitHubUrl,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Client = request.Client,
                    Budget = request.Budget,
                    
                    IsPublished = request.IsPublished,
                    IsFeatured = request.IsFeatured,
                    SortOrder = request.SortOrder,
                    CreatedByUserId = createdByUserId
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Handle multiple images upload
                if (request.Images?.Any() == true)
                {
                    var images = new List<ProjectImage>();
                    for (int i = 0; i < request.Images.Count; i++)
                    {
                        var imageFile = request.Images[i];
                        var imageResult = await _fileStorage.SaveImageAsync(imageFile, "projects");
                        if (!imageResult.Ok)
                            return Fail<ProjectDto>(imageResult.Error!.Message, imageResult.Error.InternalCode ?? 4000, null, traceId);

                        var projectImage = new ProjectImage
                        {
                            ProjectId = project.Id,
                            FilePath = imageResult.Data!.FilePath,
                            ContentType = imageResult.Data.ContentType,
                            FileName = imageResult.Data.FileName,
                            FileSize = imageResult.Data.FileSize,
                            IsMainImage = request.MainImageIndex.HasValue && request.MainImageIndex.Value == i,
                            SortOrder = i,
                            CreatedByUserId = createdByUserId
                        };

                        images.Add(projectImage);
                    }

                    if (images.Any() && !images.Any(i => i.IsMainImage))
                        images.First().IsMainImage = true;

                    _context.ProjectImages.AddRange(images);
                    await _context.SaveChangesAsync();
                }

                // Handle multiple videos upload
                if (request.Videos?.Any() == true)
                {
                    var videos = new List<ProjectVideo>();
                    for (int i = 0; i < request.Videos.Count; i++)
                    {
                        var videoFile = request.Videos[i];
                        var videoResult = await _fileStorage.SaveVideoAsync(videoFile, "projects");
                        if (!videoResult.Ok)
                            return Fail<ProjectDto>(videoResult.Error!.Message, videoResult.Error.InternalCode ?? 4000, null, traceId);

                        var projectVideo = new ProjectVideo
                        {
                            ProjectId = project.Id,
                            FilePath = videoResult.Data!.FilePath,
                            ContentType = videoResult.Data.ContentType,
                            FileName = videoResult.Data.FileName,
                            FileSize = videoResult.Data.FileSize,
                            IsMainVideo = request.MainVideoIndex.HasValue && request.MainVideoIndex.Value == i,
                            SortOrder = i,
                            CreatedByUserId = createdByUserId
                        };

                        videos.Add(projectVideo);
                    }

                    if (videos.Any() && !videos.Any(v => v.IsMainVideo))
                        videos.First().IsMainVideo = true;

                    _context.ProjectVideos.AddRange(videos);
                    await _context.SaveChangesAsync();
                }

                // Handle translations
                if (request.Translations?.Any() == true)
                {
                    foreach (var t in request.Translations)
                    {
                        var translation = new ProjectTranslation
                        {
                            ProjectId = project.Id,
                            Language = t.Language.Trim(),
                            Direction = t.Direction,
                            Title = t.Title,
                            Description = t.Description
                        };
                        _context.ProjectTranslations.Add(translation);
                    }
                    await _context.SaveChangesAsync();
                }

                // Load the project with navigation properties
                var createdProject = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images).ThenInclude(i => i.CreatedBy)
                    .Include(p => p.Videos).ThenInclude(v => v.CreatedBy)
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.Id == project.Id);

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(createdProject!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to create project", traceId);
                return Fail<ProjectDto>("Failed to create project", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id, string? language = null)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var query = _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images).ThenInclude(i => i.CreatedBy)
                    .Include(p => p.Videos).ThenInclude(v => v.CreatedBy)
                    .Include(p => p.Translations)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(language))
                {
                    query = query.Include(p => p.Translations.Where(t => t.Language == language));
                }

                var project = await query.FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                    return Fail<ProjectDto>("Project not found", 4004, null, traceId);

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project {ProjectId}", traceId, id);
                return Fail<ProjectDto>("Failed to get project", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<PagedResponse<ProjectDto>>> GetProjectsAsync(ProjectFilterDto filter)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var query = BuildProjectQuery(filter);
                var totalCount = await query.CountAsync();

                var projects = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images).ThenInclude(i => i.CreatedBy)
                    .Include(p => p.Videos).ThenInclude(v => v.CreatedBy)
                    .Include(p => p.Translations)
                    .ToListAsync();

                var data = projects.Select(ProjectDto.FromProject).ToList();

                var paged = new PagedResponse<ProjectDto>
                {
                    Data = data,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResponse<ProjectDto>>.Success(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get projects", traceId);
                return Fail<PagedResponse<ProjectDto>>("Failed to get projects", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<PagedResponse<ProjectSummaryDto>>> GetProjectsSummaryAsync(ProjectFilterDto filter)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var query = BuildProjectQuery(filter);
                var totalCount = await query.CountAsync();

                var projects = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Include(p => p.Images.Where(i => i.IsMainImage))
                    .Include(p => p.Translations)
                    .ToListAsync();

                var data = projects.Select(ProjectSummaryDto.FromProject).ToList();

                var paged = new PagedResponse<ProjectSummaryDto>
                {
                    Data = data,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResponse<ProjectSummaryDto>>.Success(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get projects summary", traceId);
                return Fail<PagedResponse<ProjectSummaryDto>>("Failed to get projects summary", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<List<ProjectSummaryDto>>> GetFeaturedProjectsAsync(int count = 6, string? language = null)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var query = _context.Projects
                    .Where(p => p.IsPublished && p.IsFeatured)
                    .Include(p => p.Images.Where(i => i.IsMainImage))
                    .Include(p => p.Translations)
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(count);

                if (!string.IsNullOrEmpty(language))
                {
                    query = query.Include(p => p.Translations.Where(t => t.Language == language));
                }

                var projects = await query.ToListAsync();
                var data = projects.Select(ProjectSummaryDto.FromProject).ToList();

                return ApiResponse<List<ProjectSummaryDto>>.Success(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get featured projects", traceId);
                return Fail<List<ProjectSummaryDto>>("Failed to get featured projects", 5000, ex, traceId);
            }
        }

        private IQueryable<Project> BuildProjectQuery(ProjectFilterDto filter)
        {
            var query = _context.Projects.AsQueryable();

            // Basic filters
            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.IsPublished.HasValue)
                query = query.Where(p => p.IsPublished == filter.IsPublished.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            // Real estate specific filters
            if (!string.IsNullOrEmpty(filter.PropertyType))
                query = query.Where(p => p.PropertyType != null && p.PropertyType.Contains(filter.PropertyType));

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(p => p.Location != null && p.Location.Contains(filter.Location));

            // Price range filters
            if (filter.PriceMin.HasValue)
                query = query.Where(p => p.PriceStart >= filter.PriceMin || p.PriceEnd >= filter.PriceMin);

            if (filter.PriceMax.HasValue)
                query = query.Where(p => p.PriceStart <= filter.PriceMax || p.PriceEnd <= filter.PriceMax);

            // Date range filters
            if (filter.StartDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= filter.StartDateFrom);

            if (filter.StartDateTo.HasValue)
                query = query.Where(p => p.StartDate <= filter.StartDateTo);

            // Search functionality
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    (p.Location != null && p.Location.ToLower().Contains(searchTerm)) ||
                    (p.PropertyType != null && p.PropertyType.ToLower().Contains(searchTerm)) ||
                    p.Translations.Any(t => 
                        t.Title.ToLower().Contains(searchTerm) ||
                        t.Description.ToLower().Contains(searchTerm)));
            }

            // Language filter
            if (!string.IsNullOrEmpty(filter.Language))
            {
                query = query.Where(p => p.Translations.Any(t => t.Language == filter.Language));
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "createdat" => filter.SortDescending 
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "name" => filter.SortDescending 
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "startdate" => filter.SortDescending 
                    ? query.OrderByDescending(p => p.StartDate)
                    : query.OrderBy(p => p.StartDate),
                "pricestart" => filter.SortDescending 
                    ? query.OrderByDescending(p => p.PriceStart)
                    : query.OrderBy(p => p.PriceStart),
                "location" => filter.SortDescending 
                    ? query.OrderByDescending(p => p.Location)
                    : query.OrderBy(p => p.Location),
                _ => filter.SortDescending 
                    ? query.OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
            };

            return query;
        }

        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectFormRequestDto request)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images).ThenInclude(i => i.CreatedBy)
                    .Include(p => p.Videos).ThenInclude(v => v.CreatedBy)
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                    return Fail<ProjectDto>("Project not found", 4004, null, traceId);

                // Update basic properties
                if (!string.IsNullOrEmpty(request.Name)) project.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Description)) project.Description = request.Description;
                if (request.Status.HasValue) project.Status = request.Status.Value;
                
                // Real estate fields
                if (request.CompanyUrl != null) project.CompanyUrl = request.CompanyUrl;
                if (request.GoogleMapsUrl != null) project.GoogleMapsUrl = request.GoogleMapsUrl;
                if (request.Location != null) project.Location = request.Location;
                if (request.PropertyType != null) project.PropertyType = request.PropertyType;
                if (request.TotalUnits.HasValue) project.TotalUnits = request.TotalUnits;
                if (request.ProjectArea.HasValue) project.ProjectArea = request.ProjectArea;
                if (request.PriceStart.HasValue) project.PriceStart = request.PriceStart;
                if (request.PriceEnd.HasValue) project.PriceEnd = request.PriceEnd;
                if (request.PriceCurrency != null) project.PriceCurrency = request.PriceCurrency;
                
                // Legacy fields
                if (request.TechnologiesUsed != null) project.TechnologiesUsed = request.TechnologiesUsed;
                if (request.ProjectUrl != null) project.ProjectUrl = request.ProjectUrl;
                if (request.GitHubUrl != null) project.GitHubUrl = request.GitHubUrl;
                if (request.StartDate.HasValue) project.StartDate = request.StartDate;
                if (request.EndDate.HasValue) project.EndDate = request.EndDate;
                if (request.Client != null) project.Client = request.Client;
                if (request.Budget.HasValue) project.Budget = request.Budget;
                
                if (request.IsPublished.HasValue) project.IsPublished = request.IsPublished.Value;
                if (request.IsFeatured.HasValue) project.IsFeatured = request.IsFeatured.Value;
                if (request.SortOrder.HasValue) project.SortOrder = request.SortOrder.Value;

                // Handle image removals
                if (request.RemoveImageIds?.Any() == true)
                {
                    var imagesToRemove = project.Images.Where(i => request.RemoveImageIds.Contains(i.Id)).ToList();
                    foreach (var imageToRemove in imagesToRemove)
                    {
                        await _fileStorage.DeleteFileAsync(imageToRemove.FilePath);
                    }
                    _context.ProjectImages.RemoveRange(imagesToRemove);
                }

                // Handle new images
                if (request.NewImages?.Any() == true)
                {
                    var maxSortOrder = project.Images.Any() ? project.Images.Max(i => i.SortOrder) : -1;
                    foreach (var imageFile in request.NewImages)
                    {
                        var imageResult = await _fileStorage.SaveImageAsync(imageFile, "projects");
                        if (!imageResult.Ok)
                            return Fail<ProjectDto>(imageResult.Error!.Message, imageResult.Error.InternalCode ?? 4000, null, traceId);

                        var projectImage = new ProjectImage
                        {
                            ProjectId = project.Id,
                            FilePath = imageResult.Data!.FilePath,
                            ContentType = imageResult.Data.ContentType,
                            FileName = imageResult.Data.FileName,
                            FileSize = imageResult.Data.FileSize,
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
                        image.IsMainImage = image.Id == request.MainImageId.Value;
                }

                // Handle video removals
                if (request.RemoveVideoIds?.Any() == true)
                {
                    var videosToRemove = project.Videos.Where(v => request.RemoveVideoIds.Contains(v.Id)).ToList();
                    foreach (var videoToRemove in videosToRemove)
                    {
                        await _fileStorage.DeleteFileAsync(videoToRemove.FilePath);
                    }
                    _context.ProjectVideos.RemoveRange(videosToRemove);
                }

                // Handle new videos
                if (request.NewVideos?.Any() == true)
                {
                    var maxVideoSort = project.Videos.Any() ? project.Videos.Max(v => v.SortOrder) : -1;
                    foreach (var videoFile in request.NewVideos)
                    {
                        var videoResult = await _fileStorage.SaveVideoAsync(videoFile, "projects");
                        if (!videoResult.Ok)
                            return Fail<ProjectDto>(videoResult.Error!.Message, videoResult.Error.InternalCode ?? 4000, null, traceId);

                        var projectVideo = new ProjectVideo
                        {
                            ProjectId = project.Id,
                            FilePath = videoResult.Data!.FilePath,
                            ContentType = videoResult.Data.ContentType,
                            FileName = videoResult.Data.FileName,
                            FileSize = videoResult.Data.FileSize,
                            IsMainVideo = false,
                            SortOrder = ++maxVideoSort
                        };
                        _context.ProjectVideos.Add(projectVideo);
                    }
                }

                // Handle main video change
                if (request.MainVideoId.HasValue)
                {
                    foreach (var video in project.Videos)
                        video.IsMainVideo = video.Id == request.MainVideoId.Value;
                }

                // Handle translations upsert
                if (request.Translations?.Any() == true)
                {
                    foreach (var t in request.Translations)
                    {
                        var lang = t.Language.Trim();
                        var existing = project.Translations.FirstOrDefault(x => x.Language == lang);
                        if (existing == null)
                        {
                            _context.ProjectTranslations.Add(new ProjectTranslation
                            {
                                ProjectId = project.Id,
                                Language = lang,
                                Direction = t.Direction,
                                Title = t.Title,
                                Description = t.Description
                            });
                        }
                        else
                        {
                            existing.Title = t.Title;
                            existing.Description = t.Description;
                            existing.Direction = t.Direction;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                project.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Reload project with updated navigation properties
                await _context.Entry(project).Collection(p => p.Images).LoadAsync();
                await _context.Entry(project).Collection(p => p.Videos).LoadAsync();
                await _context.Entry(project).Collection(p => p.Translations).LoadAsync();

                return ApiResponse<ProjectDto>.Success(ProjectDto.FromProject(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to update project {ProjectId}", traceId, id);
                return Fail<ProjectDto>("Failed to update project", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(Guid id)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .Include(p => p.Videos)
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (project == null)
                    return Fail<bool>("Project not found", 4004, null, traceId);

                // Delete files from storage
                foreach (var image in project.Images)
                {
                    await _fileStorage.DeleteFileAsync(image.FilePath);
                }

                foreach (var video in project.Videos)
                {
                    await _fileStorage.DeleteFileAsync(video.FilePath);
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to delete project {ProjectId}", traceId, id);
                return Fail<bool>("Failed to delete project", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> ToggleFeaturedAsync(Guid id)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null)
                    return Fail<bool>("Project not found", 4004, null, traceId);

                project.IsFeatured = !project.IsFeatured;
                project.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(project.IsFeatured);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to toggle featured status for project {ProjectId}", traceId, id);
                return Fail<bool>("Failed to toggle featured status", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsByStatusAsync(ProjectStatus status)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Images).ThenInclude(i => i.CreatedBy)
                    .Include(p => p.Videos).ThenInclude(v => v.CreatedBy)
                    .Include(p => p.Translations)
                    .Where(p => p.Status == status && p.IsPublished)
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var projectDtos = projects.Select(ProjectDto.FromProject).ToList();
                return ApiResponse<List<ProjectDto>>.Success(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get projects by status {Status}", traceId, status);
                return Fail<List<ProjectDto>>("Failed to get projects by status", 5000, ex, traceId);
            }
        }

        // Images implementation with file storage
        public async Task<ApiResponse<ProjectImageDto>> GetProjectImageAsync(Guid imageId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var image = await _context.ProjectImages
                    .Include(i => i.CreatedBy)
                    .FirstOrDefaultAsync(i => i.Id == imageId);

                if (image == null)
                    return Fail<ProjectImageDto>("Project image not found", 4004, null, traceId);

                return ApiResponse<ProjectImageDto>.Success(ProjectImageDto.FromProjectImage(image));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project image {ImageId}", traceId, imageId);
                return Fail<ProjectImageDto>("Failed to get project image", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<Stream>> GetProjectImageStreamAsync(Guid imageId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var image = await _context.ProjectImages.FindAsync(imageId);
                if (image == null)
                    return Fail<Stream>("Project image not found", 4004, null, traceId);

                return await _fileStorage.GetFileStreamAsync(image.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project image stream {ImageId}", traceId, imageId);
                return Fail<Stream>("Failed to get project image", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectImageAsync(Guid imageId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var image = await _context.ProjectImages.FirstOrDefaultAsync(i => i.Id == imageId);
                if (image == null)
                    return Fail<bool>("Project image not found", 4004, null, traceId);

                // Delete file from storage
                await _fileStorage.DeleteFileAsync(image.FilePath);

                _context.ProjectImages.Remove(image);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to delete project image {ImageId}", traceId, imageId);
                return Fail<bool>("Failed to delete project image", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<ProjectImageDto>> AddProjectImageAsync(Guid projectId, IFormFile image, string? description, bool isMainImage, Guid createdByUserId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Fail<ProjectImageDto>("Project not found", 4004, null, traceId);

                // Save image file
                var imageResult = await _fileStorage.SaveImageAsync(image, "projects");
                if (!imageResult.Ok)
                    return Fail<ProjectImageDto>(imageResult.Error!.Message, imageResult.Error.InternalCode ?? 4000, null, traceId);

                // If setting as main image, unset other main images
                if (isMainImage)
                {
                    foreach (var existingImage in project.Images)
                        existingImage.IsMainImage = false;
                }

                var maxSortOrder = project.Images.Any() ? project.Images.Max(i => i.SortOrder) : -1;

                var projectImage = new ProjectImage
                {
                    ProjectId = projectId,
                    FilePath = imageResult.Data!.FilePath,
                    ContentType = imageResult.Data.ContentType,
                    FileName = imageResult.Data.FileName,
                    FileSize = imageResult.Data.FileSize,
                    Description = description,
                    IsMainImage = isMainImage,
                    SortOrder = maxSortOrder + 1,
                    CreatedByUserId = createdByUserId
                };

                _context.ProjectImages.Add(projectImage);
                await _context.SaveChangesAsync();

                // Load with navigation properties
                await _context.Entry(projectImage).Reference(i => i.CreatedBy).LoadAsync();

                return ApiResponse<ProjectImageDto>.Success(ProjectImageDto.FromProjectImage(projectImage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to add project image to project {ProjectId}", traceId, projectId);
                return Fail<ProjectImageDto>("Failed to add project image", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> SetMainImageAsync(Guid projectId, Guid imageId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Fail<bool>("Project not found", 4004, null, traceId);

                var targetImage = project.Images.FirstOrDefault(i => i.Id == imageId);
                if (targetImage == null)
                    return Fail<bool>("Image not found in project", 4004, null, traceId);

                foreach (var image in project.Images)
                    image.IsMainImage = false;

                targetImage.IsMainImage = true;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to set main image for project {ProjectId}", traceId, projectId);
                return Fail<bool>("Failed to set main image", 5000, ex, traceId);
            }
        }

        // Videos implementation with file storage
        public async Task<ApiResponse<ProjectVideoDto>> GetProjectVideoAsync(Guid videoId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var video = await _context.ProjectVideos
                    .Include(v => v.CreatedBy)
                    .FirstOrDefaultAsync(v => v.Id == videoId);

                if (video == null)
                    return Fail<ProjectVideoDto>("Project video not found", 4004, null, traceId);

                return ApiResponse<ProjectVideoDto>.Success(ProjectVideoDto.FromProjectVideo(video));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project video {VideoId}", traceId, videoId);
                return Fail<ProjectVideoDto>("Failed to get project video", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<Stream>> GetProjectVideoStreamAsync(Guid videoId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var video = await _context.ProjectVideos.FindAsync(videoId);
                if (video == null)
                    return Fail<Stream>("Project video not found", 4004, null, traceId);

                return await _fileStorage.GetFileStreamAsync(video.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project video stream {VideoId}", traceId, videoId);
                return Fail<Stream>("Failed to get project video", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<ProjectVideoDto>> AddProjectVideoAsync(Guid projectId, IFormFile video, string? description, bool isMainVideo, Guid createdByUserId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Videos)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Fail<ProjectVideoDto>("Project not found", 4004, null, traceId);

                // Save video file
                var videoResult = await _fileStorage.SaveVideoAsync(video, "projects");
                if (!videoResult.Ok)
                    return Fail<ProjectVideoDto>(videoResult.Error!.Message, videoResult.Error.InternalCode ?? 4000, null, traceId);

                if (isMainVideo)
                {
                    foreach (var existingVideo in project.Videos)
                        existingVideo.IsMainVideo = false;
                }

                var maxSortOrder = project.Videos.Any() ? project.Videos.Max(v => v.SortOrder) : -1;

                var projectVideo = new ProjectVideo
                {
                    ProjectId = projectId,
                    FilePath = videoResult.Data!.FilePath,
                    ContentType = videoResult.Data.ContentType,
                    FileName = videoResult.Data.FileName,
                    FileSize = videoResult.Data.FileSize,
                    Description = description,
                    IsMainVideo = isMainVideo,
                    SortOrder = maxSortOrder + 1,
                    CreatedByUserId = createdByUserId
                };

                _context.ProjectVideos.Add(projectVideo);
                await _context.SaveChangesAsync();

                await _context.Entry(projectVideo).Reference(v => v.CreatedBy).LoadAsync();

                return ApiResponse<ProjectVideoDto>.Success(ProjectVideoDto.FromProjectVideo(projectVideo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to add project video to project {ProjectId}", traceId, projectId);
                return Fail<ProjectVideoDto>("Failed to add project video", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectVideoAsync(Guid videoId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var video = await _context.ProjectVideos.FirstOrDefaultAsync(v => v.Id == videoId);
                if (video == null)
                    return Fail<bool>("Project video not found", 4004, null, traceId);

                // Delete file from storage
                await _fileStorage.DeleteFileAsync(video.FilePath);

                _context.ProjectVideos.Remove(video);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to delete project video {VideoId}", traceId, videoId);
                return Fail<bool>("Failed to delete project video", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> SetMainVideoAsync(Guid projectId, Guid videoId)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Videos)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Fail<bool>("Project not found", 4004, null, traceId);

                var targetVideo = project.Videos.FirstOrDefault(v => v.Id == videoId);
                if (targetVideo == null)
                    return Fail<bool>("Video not found in project", 4004, null, traceId);

                foreach (var video in project.Videos)
                    video.IsMainVideo = false;

                targetVideo.IsMainVideo = true;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to set main video for project {ProjectId}", traceId, projectId);
                return Fail<bool>("Failed to set main video", 5000, ex, traceId);
            }
        }

        // Translation methods remain the same
        public async Task<ApiResponse<ProjectTranslationDto>> UpsertProjectTranslationAsync(Guid projectId, ProjectTranslationUpsertDto translation)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Fail<ProjectTranslationDto>("Project not found", 4004, null, traceId);

                var lang = translation.Language.Trim();
                var existing = project.Translations.FirstOrDefault(x => x.Language == lang);
                
                if (existing == null)
                {
                    var newTranslation = new ProjectTranslation
                    {
                        ProjectId = projectId,
                        Language = lang,
                        Direction = translation.Direction,
                        Title = translation.Title,
                        Description = translation.Description
                    };
                    _context.ProjectTranslations.Add(newTranslation);
                    await _context.SaveChangesAsync();
                    
                    return ApiResponse<ProjectTranslationDto>.Success(ProjectTranslationDto.FromTranslation(newTranslation));
                }
                else
                {
                    existing.Title = translation.Title;
                    existing.Description = translation.Description;
                    existing.Direction = translation.Direction;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    return ApiResponse<ProjectTranslationDto>.Success(ProjectTranslationDto.FromTranslation(existing));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to upsert project translation for project {ProjectId}", traceId, projectId);
                return Fail<ProjectTranslationDto>("Failed to upsert project translation", 5000, ex, traceId);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectTranslationAsync(Guid projectId, string language)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var translation = await _context.ProjectTranslations
                    .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Language == language);

                if (translation == null)
                    return Fail<bool>("Translation not found", 4004, null, traceId);

                _context.ProjectTranslations.Remove(translation);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to delete project translation for project {ProjectId}", traceId, projectId);
                return Fail<bool>("Failed to delete project translation", 5000, ex, traceId);
            }
        }

        // Analytics method remains the same
        public async Task<ApiResponse<object>> GetProjectStatisticsAsync()
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                var stats = new
                {
                    TotalProjects = await _context.Projects.CountAsync(),
                    PublishedProjects = await _context.Projects.CountAsync(p => p.IsPublished),
                    FeaturedProjects = await _context.Projects.CountAsync(p => p.IsFeatured),
                    ProjectsByStatus = await _context.Projects
                        .GroupBy(p => p.Status)
                        .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                        .ToListAsync(),
                    ProjectsByPropertyType = await _context.Projects
                        .Where(p => p.PropertyType != null)
                        .GroupBy(p => p.PropertyType)
                        .Select(g => new { PropertyType = g.Key, Count = g.Count() })
                        .ToListAsync(),
                    ProjectsByLocation = await _context.Projects
                        .Where(p => p.Location != null)
                        .GroupBy(p => p.Location)
                        .Select(g => new { Location = g.Key, Count = g.Count() })
                        .ToListAsync(),
                    TotalImages = await _context.ProjectImages.CountAsync(),
                    TotalVideos = await _context.ProjectVideos.CountAsync(),
                    TotalTranslations = await _context.ProjectTranslations.CountAsync(),
                    LanguageDistribution = await _context.ProjectTranslations
                        .GroupBy(t => t.Language)
                        .Select(g => new { Language = g.Key, Count = g.Count() })
                        .ToListAsync(),
                    GeneratedAt = DateTime.UtcNow
                };

                return ApiResponse<object>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get project statistics", traceId);
                return Fail<object>("Failed to get project statistics", 5000, ex, traceId);
            }
        }

        private ApiResponse<T> Fail<T>(string message, int internalCode, Exception? ex = null, string? traceId = null)
        {
            if (ex != null)
                _logger.LogError(ex, message);

            return new ApiResponse<T>
            {
                Ok = false,
                Error = new ErrorDetails
                {
                    Message = message,
                    InternalCode = internalCode,
                    Details = ex?.ToString(),
                    TraceId = traceId
                }
            };
        }
    }
}