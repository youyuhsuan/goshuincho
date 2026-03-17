using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Requests
{
    public class AuthorizationRequest
    {
        public required string Provider { get; set; }
    }

    public class OAuthRequest
    {
        public required string Provider { get; set; }
        public required string Code { get; set; }
        public required string State { get; set; }
    }
}