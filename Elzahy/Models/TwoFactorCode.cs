using System.ComponentModel.DataAnnotations;

namespace Elzahy.Models
{
    public class TwoFactorCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? UsedAt { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Purpose { get; set; } = string.Empty; // "Login", "PasswordReset", "EmailConfirmation"
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}