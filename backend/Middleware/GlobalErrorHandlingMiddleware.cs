using System.Text.Json;

using backend.Exceptions;
using backend.Models;


namespace backend.Middleware
{
    // Global middleware for handling unhandled exceptions throughout the application
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        // Initializes the global error handling middleware
        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Processes HTTP requests and handles any unhandled exceptions
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                // Handle the exception and return a standardized error response
                await HandleExceptionAsync(context, ex);
            }
        }

        // Handles exceptions by mapping them to appropriate HTTP status codes and error messages
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogInformation("Handling exception of type: {ExceptionType}", exception.GetType().Name);

            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ValidationException validationEx => (400, (object)validationEx.Errors),
                UnauthorizedAccessException unauthorizedAccessEx => (401, (object)unauthorizedAccessEx.Message),
                NotFoundException notFoundEx => (404, (object)notFoundEx.Message),
                ForbiddenException forbiddenEx => (403, (object)forbiddenEx.Message),
                ConflictException conflictEx => (409, (object)conflictEx.Message),
                UnprocessableContent unprocessableContentEx => (422, (object)unprocessableContentEx.Message),
                _ => (500, (object)"Internal server error")
            };

            // Set the HTTP status code
            context.Response.StatusCode = statusCode;

            // Serialize and write the JSON response with camelCase property naming
            await context.Response.WriteAsync(JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}