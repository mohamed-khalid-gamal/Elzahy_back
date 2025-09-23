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

    // Form data DTOs for handling file uploads
    public class CreateProjectFormRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public List<IFormFile>? Images { get; set; }
        public int? MainImageIndex { get; set; } // Index of the main image in the Images list
        
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

    public class UpdateProjectFormRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<Guid>? RemoveImageIds { get; set; }
        public Guid? MainImageId { get; set; }
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