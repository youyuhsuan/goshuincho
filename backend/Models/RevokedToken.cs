using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class RevokedToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Jti { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; }

        public DateTime RevokedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        [MaxLength(50)]
        public string? Reason { get; set; }  // "logout", "security", "admin"
    }
}