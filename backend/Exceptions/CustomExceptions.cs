namespace backend.Exceptions
{
    // Custom exception for handling validation errors
    public class ValidationException : Exception
    {
        public object Errors { get; }

        public ValidationException(object errors, string message = "Validation failed") : base(message)
        {
            Errors = errors;
        }
    }

    // Custom exception for handling unauthorized access scenarios
    // Thrown when authentication fails or credentials are invalid (HTTP 401)
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Unauthorized access") : base(message) { }
    }

    // Custom exception for handling resource not found scenarios
    // Thrown when a requested resource doesn't exist (HTTP 404)
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // Custom exception for handling forbidden access scenarios
    // Thrown when a user doesn't have permission to access a resource (HTTP 403)
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    // Custom exception for handling resource conflict scenarios
    // Thrown when an operation conflicts with the current state of a resource (HTTP 409)
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}