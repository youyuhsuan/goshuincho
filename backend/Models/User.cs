using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public sealed class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string? GoogleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(6)]
        [MaxLength(500)]
        public string? Password { get; set; }

        [MaxLength(500)]
        public string? Picture { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? FavoriteGoods { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}