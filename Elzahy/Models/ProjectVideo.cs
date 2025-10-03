using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Elzahy.Models
{
    public class ProjectVideo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty; // Path to file on disk

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; } // File size in bytes

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsMainVideo { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        // Navigation
        [Required]
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid? CreatedByUserId { get; set; }
        public virtual User? CreatedBy { get; set; }

        // Computed properties for URLs
        public string WebUrl => $"/api/projects/videos/{Id}";
        public string FullPath => Path.Combine("wwwroot", "uploads", "videos", $"{Id}{Path.GetExtension(FileName)}");
    }
}
