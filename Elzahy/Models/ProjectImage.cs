using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class ProjectImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsMainImage { get; set; } = false;
        
        public int SortOrder { get; set; } = 0;
        
        // Navigation properties
        [Required]
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;
        
        public Guid? CreatedByUserId { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}