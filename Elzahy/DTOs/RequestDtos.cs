using System.ComponentModel.DataAnnotations;
using Elzahy.Models;

namespace Elzahy.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public bool Terms { get; set; }

        public bool RequestAdminRole { get; set; } = false;
    }
    
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;

        public string? TwoFactorCode { get; set; }
    }
    
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
    
    public class UpdateUserRequestDto
    {
        public string? Name { get; set; }
        public string? Language { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AdminRegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "User"; // "User" or "Admin"
    }

    public class AdminRequestDto
    {
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }
    }

    public class AdminRequestApprovalDto
    {
        [Required]
        public bool Approved { get; set; }
        
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    public class Enable2FARequestDto
    {
        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
    }

    public class Verify2FARequestDto
    {
        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Purpose { get; set; } = string.Empty;
    }

    public class TempTokenVerifyRequestDto
    {
        [Required]
        public string TempToken { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
    }

    public class RecoveryCodeVerifyRequestDto
    {
        [Required]
        public string TempToken { get; set; } = string.Empty;
        
        [Required]
        public string RecoveryCode { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    // Legacy DTOs for backward compatibility
    public class CreateProjectRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public ProjectStatus Status { get; set; }
        
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        public bool IsPublished { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateProjectRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus? Status { get; set; }
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateAwardRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string GivenBy { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateReceived { get; set; }
        
        public string? Description { get; set; }
        public string? CertificateUrl { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublished { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateAwardRequestDto
    {
        public string? Name { get; set; }
        public string? GivenBy { get; set; }
        public DateTime? DateReceived { get; set; }
        public string? Description { get; set; }
        public string? CertificateUrl { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateContactMessageRequestDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string EmailAddress { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        public string? Company { get; set; }
    }

    public class UpdateContactMessageRequestDto
    {
        public bool? IsRead { get; set; }
        public bool? IsReplied { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class ContactMessageFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsReplied { get; set; }
        public string? SortBy { get; set; } = "CreatedAt"; // CreatedAt, Subject, FullName
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // Enhanced Real Estate Project DTOs
    public class CreateProjectFormRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public List<IFormFile>? Images { get; set; }
        public int? MainImageIndex { get; set; } // Index of the main image in the Images list
        public List<IFormFile>? Videos { get; set; }
        public int? MainVideoIndex { get; set; }
        
        [Required]
        public ProjectStatus Status { get; set; }
        
        // Real Estate Specific Fields
        public string? CompanyUrl { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? Location { get; set; }
        public string? PropertyType { get; set; }
        public int? TotalUnits { get; set; }
        public decimal? ProjectArea { get; set; }
        public decimal? PriceStart { get; set; }
        public decimal? PriceEnd { get; set; }
        public string? PriceCurrency { get; set; } = "EGP";
        
        // Legacy fields for compatibility
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        
        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        // Optional initial translations
        public List<ProjectTranslationUpsertDto>? Translations { get; set; }
    }

    public class UpdateProjectFormRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<Guid>? RemoveImageIds { get; set; }
        public Guid? MainImageId { get; set; }

        public List<IFormFile>? NewVideos { get; set; }
        public List<Guid>? RemoveVideoIds { get; set; }
        public Guid? MainVideoId { get; set; }

        public ProjectStatus? Status { get; set; }
        
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
        
        // Legacy compatibility
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Client { get; set; }
        public decimal? Budget { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public int? SortOrder { get; set; }

        public List<ProjectTranslationUpsertDto>? Translations { get; set; }
    }

    public class ProjectTranslationUpsertDto
    {
        [Required]
        [StringLength(10)]
        public string Language { get; set; } = "ar";
        
        [Required]
        public TextDirection Direction { get; set; } = TextDirection.RTL;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
    }

    // Enhanced Project Filter DTO for better search and pagination
    public class ProjectFilterDto
    {
        public ProjectStatus? Status { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public string? PropertyType { get; set; }
        public string? Location { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public string? SearchTerm { get; set; } // Search in name, description, location
        public string? Language { get; set; } // Filter by translation language
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public string? SortBy { get; set; } = "SortOrder"; // SortOrder, CreatedAt, Name, StartDate, PriceStart
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12; // Default to 12 for better grid layout
    }

    public class CreateAwardFormRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string GivenBy { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateReceived { get; set; }
        
        public string? Description { get; set; }
        public string? CertificateUrl { get; set; }
        public IFormFile? Image { get; set; }
        public bool IsPublished { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateAwardFormRequestDto
    {
        public string? Name { get; set; }
        public string? GivenBy { get; set; }
        public DateTime? DateReceived { get; set; }
        public string? Description { get; set; }
        public string? CertificateUrl { get; set; }
        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; } = false;
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }
}