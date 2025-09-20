namespace backend.Common
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
}