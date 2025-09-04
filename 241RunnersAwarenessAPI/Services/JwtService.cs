using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetJwtKey()
        {
            // Try configuration first, then environment variables, then fallback
            return _configuration["Jwt:Key"] 
                ?? Environment.GetEnvironmentVariable("JWT__Key")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Key")
                ?? "your-super-secret-key-with-at-least-32-characters";
        }

        private string GetJwtIssuer()
        {
            return _configuration["Jwt:Issuer"] 
                ?? Environment.GetEnvironmentVariable("JWT__Issuer")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Issuer")
                ?? "241RunnersAwareness";
        }

        private string GetJwtAudience()
        {
            return _configuration["Jwt:Audience"] 
                ?? Environment.GetEnvironmentVariable("JWT__Audience")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Audience")
                ?? "241RunnersAwareness";
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: GetJwtIssuer(),
                audience: GetJwtAudience(),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtKey());

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetJwtIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetJwtAudience(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
} 