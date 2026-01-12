namespace backend.Models.Requests
{
    public class TokenRequest
    {
        public required string Provider { get; set; }
        public required string Code { get; set; }
        public required string State { get; set; }
    }
}