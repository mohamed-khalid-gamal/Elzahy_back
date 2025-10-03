using System.Text.Json;
using Elzahy.Models;

namespace Elzahy.DTOs
{
    public class ApiResponse<T>
    {
        public bool Ok { get; set; }
        public T? Data { get; set; }
        public ErrorDetails? Error { get; set; }
        
        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T> { Ok = true, Data = data };
        }
        
        public static ApiResponse<T> Failure(string message, int? internalCode = null)
        {
            return new ApiResponse<T> 
            { 
                Ok = false, 
                Error = new ErrorDetails { Message = message, InternalCode = internalCode }
            };
        }
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public int? NextPage => HasNext ? PageNumber + 1 : null;
        public int? PrevPage => HasPrevious ? PageNumber - 1 : null;
    }
    
    public class ErrorDetails
    {
        public string Message { get; set; } = string.Empty;
        public int? InternalCode { get; set; }
        public string? Details { get; set; }
        public string? TraceId { get; set; }
    }
    
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserDto? User { get; set; } = null;
        public bool RequiresTwoFactor { get; set; } = false;
        public string? TempToken { get; set; }
        public int ExpiresIn { get; set; } = 0;
    }
    
    public class TokenRefreshResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 0;
    }
    
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        
        public static UserDto FromUser(Models.User user)
        {
            return new UserDto
            {
                Id = user.Id.ToString(),
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = user.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Email = user.Email,
                Name = user.Name,
                Language = user.Language,
                Role = user.Role,
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailConfirmed = user.EmailConfirmed
            };
        }
    }

    public class Setup2FAResponseDto
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeImage { get; set; } = string.Empty; // Base64 encoded PNG
        public string ManualEntryKey { get; set; } = string.Empty;
    }

    public class Enable2FAResponseDto
    {
        public bool Success { get; set; }
        public List<string> RecoveryCodes { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class RecoveryCodesResponseDto
    {
        public List<string> RecoveryCodes { get; set; } = new();
        public int Count { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProjectTranslationDto
    {
        public string Language { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty; // RTL or LTR
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public static ProjectTranslationDto FromTranslation(ProjectTranslation t)
        {
            return new ProjectTranslationDto
            {
                Language = t.Language,
                Direction = t.Direction.ToString(),
                Title = t.Title,
                Description = t.Description
            };
        }
    }

    public class ProjectVideoDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty; // URL to video file
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public bool IsMainVideo { get; set; }
        public int SortOrder { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string? CreatedByName { get; set; }

        public static ProjectVideoDto FromProjectVideo(ProjectVideo video)
        {
            return new ProjectVideoDto
            {
                Id = video.Id.ToString(),
                CreatedAt = video.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = video.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                VideoUrl = video.WebUrl,
                ContentType = video.ContentType,
                FileName = video.FileName,
                FileSize = video.FileSize,
                Description = video.Description,
                IsMainVideo = video.IsMainVideo,
                SortOrder = video.SortOrder,
                ProjectId = video.ProjectId.ToString(),
                CreatedByName = video.CreatedBy?.Name
            };
        }
    }

    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProjectImageDto> Images { get; set; } = new();
        public ProjectImageDto? MainImage { get; set; }
        public List<ProjectVideoDto> Videos { get; set; } = new();
        public ProjectVideoDto? MainVideo { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Real Estate Specific Fields
        public string? CompanyUrl { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? Location { get; set; }
        public string? PropertyType { get; set; }
        public int? TotalUnits { get; set; }
        public decimal? ProjectArea { get; set; }
        public decimal? PriceStart { get; set; }
        public decimal? PriceEnd { get; set; }
        public string? PriceCurrency { get; set; }
        public string? PriceRange => FormatPriceRange();
        
        // Legacy fields for compatibility
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public string? CreatedByName { get; set; }
        public List<ProjectTranslationDto> Translations { get; set; } = new();

        private string? FormatPriceRange()
        {
            if (!PriceStart.HasValue && !PriceEnd.HasValue) return null;
            
            var currency = PriceCurrency ?? "EGP";
            if (PriceStart.HasValue && PriceEnd.HasValue)
            {
                if (PriceStart == PriceEnd)
                    return $"{PriceStart:N0} {currency}";
                return $"{PriceStart:N0} - {PriceEnd:N0} {currency}";
            }
            if (PriceStart.HasValue)
                return $"?? {PriceStart:N0} {currency}";
            if (PriceEnd.HasValue)
                return $"??? {PriceEnd:N0} {currency}";
            return null;
        }

        public static ProjectDto FromProject(Project project)
        {
            var projectDto = new ProjectDto
            {
                Id = project.Id.ToString(),
                CreatedAt = project.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = project.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Name = project.Name,
                Description = project.Description,
                Status = project.Status.ToString(),
                
                // Real Estate Fields
                CompanyUrl = project.CompanyUrl,
                GoogleMapsUrl = project.GoogleMapsUrl,
                Location = project.Location,
                PropertyType = project.PropertyType,
                TotalUnits = project.TotalUnits,
                ProjectArea = project.ProjectArea,
                PriceStart = project.PriceStart,
                PriceEnd = project.PriceEnd,
                PriceCurrency = project.PriceCurrency,
                
                // Legacy fields
                TechnologiesUsed = project.TechnologiesUsed,
                ProjectUrl = project.ProjectUrl,
                GitHubUrl = project.GitHubUrl,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Client = project.Client,
                Budget = project.Budget,
                
                IsPublished = project.IsPublished,
                IsFeatured = project.IsFeatured,
                SortOrder = project.SortOrder,
                CreatedByName = project.CreatedBy?.Name
            };

            if (project.Images?.Any() == true)
            {
                projectDto.Images = project.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(ProjectImageDto.FromProjectImage)
                    .ToList();
                    
                projectDto.MainImage = project.Images
                    .Where(i => i.IsMainImage)
                    .OrderBy(i => i.SortOrder)
                    .Select(ProjectImageDto.FromProjectImage)
                    .FirstOrDefault();
            }

            if (project.Videos?.Any() == true)
            {
                projectDto.Videos = project.Videos
                    .OrderBy(v => v.SortOrder)
                    .Select(ProjectVideoDto.FromProjectVideo)
                    .ToList();

                projectDto.MainVideo = project.Videos
                    .Where(v => v.IsMainVideo)
                    .OrderBy(v => v.SortOrder)
                    .Select(ProjectVideoDto.FromProjectVideo)
                    .FirstOrDefault();
            }

            if (project.Translations?.Any() == true)
            {
                projectDto.Translations = project.Translations
                    .Select(ProjectTranslationDto.FromTranslation)
                    .ToList();
            }

            return projectDto;
        }
    }

    public class AwardDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GivenBy { get; set; } = string.Empty;
        public DateTime DateReceived { get; set; }
        public string? Description { get; set; }
        public string? CertificateUrl { get; set; }
        public string? ImageData { get; set; } // Base64 encoded image
        public string? ImageContentType { get; set; }
        public string? ImageFileName { get; set; }
        public bool IsPublished { get; set; }
        public int SortOrder { get; set; }
        public string? CreatedByName { get; set; }

        public static AwardDto FromAward(Award award)
        {
            return new AwardDto
            {
                Id = award.Id.ToString(),
                CreatedAt = award.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = award.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Name = award.Name,
                GivenBy = award.GivenBy,
                DateReceived = award.DateReceived,
                Description = award.Description,
                CertificateUrl = award.CertificateUrl,
                ImageData = award.ImageData != null ? Convert.ToBase64String(award.ImageData) : null,
                ImageContentType = award.ImageContentType,
                ImageFileName = award.ImageFileName,
                IsPublished = award.IsPublished,
                SortOrder = award.SortOrder,
                CreatedByName = award.CreatedBy?.Name
            };
        }
    }

    public class ContactMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsReplied { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Company { get; set; }
        public string? AdminNotes { get; set; }

        public static ContactMessageDto FromContactMessage(ContactMessage message)
        {
            return new ContactMessageDto
            {
                Id = message.Id.ToString(),
                CreatedAt = message.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                FullName = message.FullName,
                EmailAddress = message.EmailAddress,
                Subject = message.Subject,
                Message = message.Message,
                IsRead = message.IsRead,
                IsReplied = message.IsReplied,
                ReadAt = message.ReadAt,
                RepliedAt = message.RepliedAt,
                PhoneNumber = message.PhoneNumber,
                Company = message.Company,
                AdminNotes = message.AdminNotes
            };
        }
    }

    public class RealtimeUsersResponseDto
    {
        public int ActiveUsers { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    public class ProjectImageDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty; // URL to image file
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public bool IsMainImage { get; set; }
        public int SortOrder { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string? CreatedByName { get; set; }

        public static ProjectImageDto FromProjectImage(ProjectImage image)
        {
            return new ProjectImageDto
            {
                Id = image.Id.ToString(),
                CreatedAt = image.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = image.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                ImageUrl = image.WebUrl,
                ContentType = image.ContentType,
                FileName = image.FileName,
                FileSize = image.FileSize,
                Description = image.Description,
                IsMainImage = image.IsMainImage,
                SortOrder = image.SortOrder,
                ProjectId = image.ProjectId.ToString(),
                CreatedByName = image.CreatedBy?.Name
            };
        }
    }

    public class AwardImageDto
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class AdminRequestResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public bool IsApproved { get; set; }
        public bool IsProcessed { get; set; }
        public string? AdminNotes { get; set; }
        public string? ProcessedByAdminName { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string CreatedAt { get; set; } = string.Empty;

        public static AdminRequestResponseDto FromAdminRequest(AdminRequest request)
        {
            return new AdminRequestResponseDto
            {
                Id = request.Id.ToString(),
                UserId = request.UserId.ToString(),
                UserName = request.User?.Name ?? "Unknown",
                UserEmail = request.User?.Email ?? "Unknown",
                Reason = request.Reason,
                AdditionalInfo = request.AdditionalInfo,
                IsApproved = request.IsApproved,
                IsProcessed = request.IsProcessed,
                AdminNotes = request.AdminNotes,
                ProcessedByAdminName = request.ProcessedByAdmin?.Name,
                ProcessedAt = request.ProcessedAt,
                CreatedAt = request.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }
    }

    public class UserManagementDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool HasPendingAdminRequest { get; set; }
        
        public static UserManagementDto FromUser(User user, bool hasPendingRequest = false)
        {
            return new UserManagementDto
            {
                Id = user.Id.ToString(),
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = user.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Email = user.Email,
                Name = user.Name,
                Language = user.Language,
                Role = user.Role,
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailConfirmed = user.EmailConfirmed,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockoutEnd = user.LockoutEnd,
                HasPendingAdminRequest = hasPendingRequest
            };
        }
    }

    // Real Estate Summary DTOs for quick overview
    public class ProjectSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? PropertyType { get; set; }
        public string? PriceRange { get; set; }
        public ProjectImageDto? MainImage { get; set; }
        public bool IsFeatured { get; set; }
        public List<ProjectTranslationDto> Translations { get; set; } = new();
        
        public static ProjectSummaryDto FromProject(Project project)
        {
            var summary = new ProjectSummaryDto
            {
                Id = project.Id.ToString(),
                Name = project.Name,
                Status = project.Status.ToString(),
                Location = project.Location,
                PropertyType = project.PropertyType,
                IsFeatured = project.IsFeatured
            };

            // Format price range
            if (project.PriceStart.HasValue || project.PriceEnd.HasValue)
            {
                var currency = project.PriceCurrency ?? "EGP";
                if (project.PriceStart.HasValue && project.PriceEnd.HasValue)
                {
                    summary.PriceRange = project.PriceStart == project.PriceEnd 
                        ? $"{project.PriceStart:N0} {currency}"
                        : $"{project.PriceStart:N0} - {project.PriceEnd:N0} {currency}";
                }
                else if (project.PriceStart.HasValue)
                {
                    summary.PriceRange = $"?? {project.PriceStart:N0} {currency}";
                }
                else if (project.PriceEnd.HasValue)
                {
                    summary.PriceRange = $"??? {project.PriceEnd:N0} {currency}";
                }
            }

            // Main image
            if (project.Images?.Any() == true)
            {
                var mainImage = project.Images.FirstOrDefault(i => i.IsMainImage) ?? project.Images.First();
                summary.MainImage = ProjectImageDto.FromProjectImage(mainImage);
            }

            // Translations
            if (project.Translations?.Any() == true)
            {
                summary.Translations = project.Translations
                    .Select(ProjectTranslationDto.FromTranslation)
                    .ToList();
            }

            return summary;
        }
    }

    // File storage result DTOs
    public class FileUploadResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string WebUrl { get; set; } = string.Empty;
    }
}