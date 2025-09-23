using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public enum ProjectStatus
    {
        Current,
        Future,
        Past
    }

    public class Project
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
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public ProjectStatus Status { get; set; }
        
        [StringLength(500)]
        public string? TechnologiesUsed { get; set; }
        
        [StringLength(500)]
        public string? ProjectUrl { get; set; }
        
        [StringLength(500)]
        public string? GitHubUrl { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        [StringLength(100)]
        public string? Client { get; set; }
        
        public decimal? Budget { get; set; }
        
        public bool IsPublished { get; set; } = true;
        
        public int SortOrder { get; set; } = 0;
        
        // Navigation properties
        public Guid? CreatedByUserId { get; set; }
        public virtual User? CreatedBy { get; set; }
        
        // Multiple images support
        public virtual ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
    }
}