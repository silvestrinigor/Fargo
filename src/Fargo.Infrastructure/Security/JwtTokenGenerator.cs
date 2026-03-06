using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Security
{
    public class JwtTokenGenerator(IConfiguration configuration) : ITokenGenerator
    {
        public TokenGenerateResult Generate(User user)
        {
            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    );

            var credentials = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256
                    );

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Guid.ToString()),

                new(ClaimTypes.Name, user.Nameid.ToString() ?? ""),

                new(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
                };

            var expiresAt = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expiresAt,
                    signingCredentials: credentials
                    );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenGenerateResult(
                    new Token(tokenString),
                    expiresAt
                    );
        }
    }
}