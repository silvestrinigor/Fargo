using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Security;

public class JwtTokenGenerator(IConfiguration configuration) : ITokenGenerator
{
        public AuthResult Generate(User user)
        {
                var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                        );

                var credentials = new SigningCredentials(
                        key, SecurityAlgorithms.HmacSha256
                        );

                var claims = new List<Claim>
        {
            // ✅ standard .NET claim for user id
            new(ClaimTypes.NameIdentifier, user.Guid.ToString()),

            // ✅ nice to keep a username/nameid too
            new(ClaimTypes.Name, user.Nameid.ToString() ?? ""),

            // (optional) also keep JWT "sub" as the guid for interoperability
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

                return new AuthResult(
                        new JwtSecurityTokenHandler().WriteToken(token),
                        expiresAt
                        );
        }
}