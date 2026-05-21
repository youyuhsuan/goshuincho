using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace backend.Filters
{
    /// <summary>
    /// Swagger operation filter that automatically adds security requirements and response codes
    /// for endpoints that require authorization.
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the controller class or method has the [Authorize] attribute
            // This combines attributes from both the controller level and method level
            var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>()
                .Any() ?? false;

            // Check if the controller class or method has the [AllowAnonymous] attribute
            // [AllowAnonymous] overrides [Authorize] requirements
            var hasAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AllowAnonymousAttribute>()
                .Any() ?? false;

            // Only apply security requirements if authorization is required and not explicitly allowed anonymous access
            if (hasAuthorize && !hasAllowAnonymous)
            {
                if (!operation.Responses.ContainsKey("401"))
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

                if (!operation.Responses.ContainsKey("403"))
                    operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }
                        ] = new string[] { }
                    }
                };
            }
        }
    }
}