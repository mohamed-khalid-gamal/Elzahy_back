using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class ContactMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
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
        
        public bool IsRead { get; set; } = false;
        public bool IsReplied { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        
        [StringLength(500)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(100)]
        public string? Company { get; set; }
        
        public string? AdminNotes { get; set; }
    }
}