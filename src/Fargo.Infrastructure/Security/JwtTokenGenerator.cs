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
    public AuthResultModel Generate(User user)
    {
        var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                );

        var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256
                );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };

        var expiresAt = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
                );

        return new AuthResultModel(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt
                );
    }
}