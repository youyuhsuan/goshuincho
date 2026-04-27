using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Requests
{
    public class ShrineSearchRequest
    {
        public string? Shrine { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}