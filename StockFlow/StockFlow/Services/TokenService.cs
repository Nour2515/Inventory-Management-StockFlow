using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Interfaces;
using StockFlow.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StockFlow.Services
{
    //Generate JWT
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        public TokenService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = jwtSettings["Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            var expirationMinutes = int.Parse(
                jwtSettings["AccessTokenMinutes"] ?? "15");

            var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email ?? string.Empty),

            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}")
        };

            var roles = _userManager
                .GetRolesAsync(user)
                .GetAwaiter()
                .GetResult();

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //Random Secure Token     
        public RefreshToken GenerateRefreshToken(User user)
        {
            return new RefreshToken
            {
                UserId = user.Id,

                Token = Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64)),

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}
