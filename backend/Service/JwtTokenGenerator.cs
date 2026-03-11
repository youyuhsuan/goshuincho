using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Helpers;

namespace backend.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string sessionId, string userId, string email, string name, DateTime? expiresAt)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, sessionId),  // Unique token ID, used for blacklisting
                new Claim(JwtRegisteredClaimNames.Sub, userId),      // Subject: user ID
                new Claim(JwtRegisteredClaimNames.Email, email),     // User email
                new Claim(JwtRegisteredClaimNames.Name, name),       // User display name
                new Claim(JwtRegisteredClaimNames.Iat,               // Issued at timestamp
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };
            // Load RSA private key and create signing credentials
            var rsa = RsaKeyHelper.LoadPrivateKey(_configuration);
            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );

            // Build the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}