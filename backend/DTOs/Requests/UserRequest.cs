using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Requests
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public List<string>? FavoriteGoods { get; set; }

        public DateTime? BirthDate { get; set; }
    }
}