using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class TokenBlacklist
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Jti { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}