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
        public string? Password { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Picture { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}