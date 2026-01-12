namespace backend.Models.Responses
{
    public class UserInfo
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    public class GoogleUserInfo
    {
        public required string id { get; set; }
        public required string email { get; set; }
        public string name { get; set; } = string.Empty;
        public string picture { get; set; } = string.Empty;
        public required bool verified_email { get; set; }
    }

    public class GoogleTokenResponse
    {
        public required string access_token { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public required int expires_in { get; set; }
        public required string scope { get; set; }

        public required string token_type { get; set; }

        public required string id_token { get; set; }
    }
}