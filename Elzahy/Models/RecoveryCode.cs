using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class RecoveryCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CodeHash { get; set; } = string.Empty;
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? UsedAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}