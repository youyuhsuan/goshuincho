using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace backend.Models
{
    public class Session
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => IsActive && !IsExpired;
    }
}