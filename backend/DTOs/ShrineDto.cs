using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class ShrineSuggestionDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ShrineDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Prefecture { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

}