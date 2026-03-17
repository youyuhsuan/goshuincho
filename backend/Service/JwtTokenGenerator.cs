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

        public string GenerateAccessToken(string userId, string email, string name)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // Unique user ID, used for blacklisting
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
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
        {
            var rsa = RsaKeyHelper.LoadPublicKey(_configuration);
            var key = new RsaSecurityKey(rsa);

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }

        public string GenerateRefreshToken(string userId, DateTime? expiresAt)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // Unique user ID, used for blacklisting
                new Claim(JwtRegisteredClaimNames.Sub, userId),      // Subject: user ID
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