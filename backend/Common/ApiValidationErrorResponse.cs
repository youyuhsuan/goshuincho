namespace backend.Common
{
    public class ValidationErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}