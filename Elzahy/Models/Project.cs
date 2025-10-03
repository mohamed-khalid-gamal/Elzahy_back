using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public enum ProjectStatus
    {
        Current,        // Under Construction / Current Projects
        Future,         // Planned / Future Projects
        Past           // Completed / Past Projects
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
        
        // Real Estate Specific Fields
        [StringLength(500)]
        public string? CompanyUrl { get; set; }

        [StringLength(500)]
        public string? GoogleMapsUrl { get; set; }

        [StringLength(200)]
        public string? Location { get; set; } // Property location

        [StringLength(100)]
        public string? PropertyType { get; set; } // e.g., Residential, Commercial, Mixed-use

        public int? TotalUnits { get; set; } // Number of units in the project

        public decimal? ProjectArea { get; set; } // Total area in square meters

        public decimal? PriceStart { get; set; } // Starting price

        public decimal? PriceEnd { get; set; } // Ending price

        [StringLength(10)]
        public string? PriceCurrency { get; set; } = "EGP"; // Default to Egyptian Pound
        
        // Legacy fields for compatibility (marked for potential future removal)
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
        
        public bool IsFeatured { get; set; } = false; // Featured projects
        
        // Navigation properties
        public Guid? CreatedByUserId { get; set; }
        public virtual User? CreatedBy { get; set; }
        
        // Multiple images support
        public virtual ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();

        // Multiple videos support
        public virtual ICollection<ProjectVideo> Videos { get; set; } = new List<ProjectVideo>();

        // Translations for multi-language Title/Description
        public virtual ICollection<ProjectTranslation> Translations { get; set; } = new List<ProjectTranslation>();
    }
}