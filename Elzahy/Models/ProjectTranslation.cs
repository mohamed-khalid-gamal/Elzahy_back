using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public enum TextDirection
    {
        LTR,
        RTL
    }

    public class ProjectTranslation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string Language { get; set; } = "ar"; // e.g., ar, en

        [Required]
        public TextDirection Direction { get; set; } = TextDirection.RTL; // Default to RTL for Arabic

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Navigation
        [Required]
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;
    }
}
