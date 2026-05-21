using Microsoft.AspNetCore.Mvc;

namespace backend.Configuration
{
    // Static class for configuring API validation behavior
    public static class ApiConfiguration
    {
        // Extension method to configure custom API model validation response format
        public static void ConfigureApiValidation(this IServiceCollection services)
        {
            // Configure API behavior options to use custom validation error response
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = CreateValidationErrorResponse;
            });
        }

        // Creates a standardized validation error response when model validation fails
        private static IActionResult CreateValidationErrorResponse(ActionContext context)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (var field in context.ModelState)
            {
                if (field.Value.Errors.Count > 0)
                {
                    errors[field.Key] = field.Value.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                }
            }

            return new BadRequestObjectResult(errors);
        }
    }
}