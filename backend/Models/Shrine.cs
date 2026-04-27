using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public sealed class Shrine
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Prefecture { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(500)]
        public string? EnshrineDeity { get; set; }

        [MaxLength(50)]
        public string? Founded { get; set; }

        [MaxLength(100)]
        public string? OpeningHours { get; set; }

        [MaxLength(500)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        // public ICollection<UserCollection> Collections { get; set; } = [];
    }
}