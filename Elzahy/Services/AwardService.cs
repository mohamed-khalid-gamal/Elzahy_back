using Microsoft.EntityFrameworkCore;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;

namespace Elzahy.Services
{
    public interface IAwardService
    {
        Task<ApiResponse<AwardDto>> CreateAwardAsync(CreateAwardFormRequestDto request, Guid createdByUserId);
        Task<ApiResponse<AwardDto>> GetAwardAsync(Guid id);
        Task<ApiResponse<List<AwardDto>>> GetAwardsAsync(bool? isPublished = null);
        Task<ApiResponse<AwardDto>> UpdateAwardAsync(Guid id, UpdateAwardFormRequestDto request);
        Task<ApiResponse<bool>> DeleteAwardAsync(Guid id);
        Task<ApiResponse<AwardImageDto>> GetAwardImageAsync(Guid id);
    }

    public class AwardService : IAwardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AwardService> _logger;
        private readonly List<string> _allowedImageTypes = new() { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

        public AwardService(AppDbContext context, ILogger<AwardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<AwardDto>> CreateAwardAsync(CreateAwardFormRequestDto request, Guid createdByUserId)
        {
            try
            {
                var award = new Award
                {
                    Name = request.Name,
                    GivenBy = request.GivenBy,
                    DateReceived = request.DateReceived,
                    Description = request.Description,
                    CertificateUrl = request.CertificateUrl,
                    IsPublished = request.IsPublished,
                    SortOrder = request.SortOrder,
                    CreatedByUserId = createdByUserId
                };

                // Handle image upload
                if (request.Image != null)
                {
                    var imageResult = await ProcessImageAsync(request.Image);
                    if (!imageResult.Ok)
                        return ApiResponse<AwardDto>.Failure(imageResult.Error!.Message, imageResult.Error.InternalCode);

                    award.ImageData = imageResult.Data!.ImageData;
                    award.ImageContentType = imageResult.Data.ContentType;
                    award.ImageFileName = imageResult.Data.FileName;
                }

                _context.Awards.Add(award);
                await _context.SaveChangesAsync();

                // Load the award with navigation properties
                var createdAward = await _context.Awards
                    .Include(a => a.CreatedBy)
                    .FirstOrDefaultAsync(a => a.Id == award.Id);

                return ApiResponse<AwardDto>.Success(AwardDto.FromAward(createdAward!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create award");
                return ApiResponse<AwardDto>.Failure($"Failed to create award: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AwardDto>> GetAwardAsync(Guid id)
        {
            try
            {
                var award = await _context.Awards
                    .Include(a => a.CreatedBy)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (award == null)
                {
                    return ApiResponse<AwardDto>.Failure("Award not found", 4004);
                }

                return ApiResponse<AwardDto>.Success(AwardDto.FromAward(award));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get award {AwardId}", id);
                return ApiResponse<AwardDto>.Failure($"Failed to get award: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<List<AwardDto>>> GetAwardsAsync(bool? isPublished = null)
        {
            try
            {
                var query = _context.Awards.Include(a => a.CreatedBy).AsQueryable();

                if (isPublished.HasValue)
                    query = query.Where(a => a.IsPublished == isPublished.Value);

                var awards = await query
                    .OrderBy(a => a.SortOrder)
                    .ThenByDescending(a => a.DateReceived)
                    .ToListAsync();

                var awardDtos = awards.Select(AwardDto.FromAward).ToList();
                return ApiResponse<List<AwardDto>>.Success(awardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get awards");
                return ApiResponse<List<AwardDto>>.Failure($"Failed to get awards: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AwardDto>> UpdateAwardAsync(Guid id, UpdateAwardFormRequestDto request)
        {
            try
            {
                var award = await _context.Awards
                    .Include(a => a.CreatedBy)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (award == null)
                {
                    return ApiResponse<AwardDto>.Failure("Award not found", 4004);
                }

                if (!string.IsNullOrEmpty(request.Name))
                    award.Name = request.Name;

                if (!string.IsNullOrEmpty(request.GivenBy))
                    award.GivenBy = request.GivenBy;

                if (request.DateReceived.HasValue)
                    award.DateReceived = request.DateReceived.Value;

                if (request.Description != null)
                    award.Description = request.Description;

                if (request.CertificateUrl != null)
                    award.CertificateUrl = request.CertificateUrl;

                // Handle image updates
                if (request.RemoveImage)
                {
                    award.ImageData = null;
                    award.ImageContentType = null;
                    award.ImageFileName = null;
                }
                else if (request.Image != null)
                {
                    var imageResult = await ProcessImageAsync(request.Image);
                    if (!imageResult.Ok)
                        return ApiResponse<AwardDto>.Failure(imageResult.Error!.Message, imageResult.Error.InternalCode);

                    award.ImageData = imageResult.Data!.ImageData;
                    award.ImageContentType = imageResult.Data.ContentType;
                    award.ImageFileName = imageResult.Data.FileName;
                }

                if (request.IsPublished.HasValue)
                    award.IsPublished = request.IsPublished.Value;

                if (request.SortOrder.HasValue)
                    award.SortOrder = request.SortOrder.Value;

                award.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<AwardDto>.Success(AwardDto.FromAward(award));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update award {AwardId}", id);
                return ApiResponse<AwardDto>.Failure($"Failed to update award: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAwardAsync(Guid id)
        {
            try
            {
                var award = await _context.Awards.FirstOrDefaultAsync(a => a.Id == id);
                if (award == null)
                {
                    return ApiResponse<bool>.Failure("Award not found", 4004);
                }

                _context.Awards.Remove(award);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete award {AwardId}", id);
                return ApiResponse<bool>.Failure($"Failed to delete award: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<AwardImageDto>> GetAwardImageAsync(Guid id)
        {
            try
            {
                var award = await _context.Awards
                    .Where(a => a.Id == id && a.ImageData != null)
                    .Select(a => new { a.ImageData, a.ImageContentType, a.ImageFileName })
                    .FirstOrDefaultAsync();

                if (award == null || award.ImageData == null)
                {
                    return ApiResponse<AwardImageDto>.Failure("Award image not found", 4004);
                }

                var imageDto = new AwardImageDto
                {
                    ImageData = award.ImageData,
                    ContentType = award.ImageContentType ?? "application/octet-stream",
                    FileName = award.ImageFileName ?? "award-image"
                };

                return ApiResponse<AwardImageDto>.Success(imageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get award image {AwardId}", id);
                return ApiResponse<AwardImageDto>.Failure($"Failed to get award image: {ex.Message}", 5000);
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