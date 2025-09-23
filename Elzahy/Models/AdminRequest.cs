using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class AdminRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsProcessed { get; set; } = false;

        [StringLength(500)]
        public string? AdminNotes { get; set; }

        public Guid? ProcessedByAdminId { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User? ProcessedByAdmin { get; set; }
    }
}