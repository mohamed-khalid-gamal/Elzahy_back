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
    }
    
    public class ErrorDetails
    {
        public string Message { get; set; } = string.Empty;
        public int? InternalCode { get; set; }
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

    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProjectImageDto> Images { get; set; } = new();
        public ProjectImageDto? MainImage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        public bool IsPublished { get; set; }
        public int SortOrder { get; set; }
        public string? CreatedByName { get; set; }

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
                TechnologiesUsed = project.TechnologiesUsed,
                ProjectUrl = project.ProjectUrl,
                GitHubUrl = project.GitHubUrl,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Client = project.Client,
                Budget = project.Budget,
                IsPublished = project.IsPublished,
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
        public string ImageData { get; set; } = string.Empty; // Base64 encoded image
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
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
                ImageData = Convert.ToBase64String(image.ImageData),
                ContentType = image.ContentType,
                FileName = image.FileName,
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
}