using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class Award
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string GivenBy { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateReceived { get; set; }
        
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? CertificateUrl { get; set; }
        
        // Image data properties (replacing ImageUrl)
        public byte[]? ImageData { get; set; }
        
        [StringLength(100)]
        public string? ImageContentType { get; set; }
        
        [StringLength(255)]
        public string? ImageFileName { get; set; }
        
        public bool IsPublished { get; set; } = true;
        
        public int SortOrder { get; set; } = 0;
        
        // Navigation properties
        public Guid? CreatedByUserId { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}