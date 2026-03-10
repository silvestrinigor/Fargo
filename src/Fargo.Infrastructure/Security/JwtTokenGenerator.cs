using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Security
{
    public sealed class JwtTokenGenerator(
            IOptions<JwtOptions> jwtOptions
            ) : ITokenGenerator
    {
        private readonly JwtOptions options = jwtOptions.Value;

        public TokenGenerateResult Generate(User user)
        {
            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(options.Key));

            var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Guid.ToString()),
                new(ClaimTypes.Name, user.Nameid.ToString() ?? string.Empty),
                new(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
            };

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
                    options.AccessTokenExpirationInMinutes);

            var token = new JwtSecurityToken(
                    issuer: options.Issuer,
                    audience: options.Audience,
                    claims: claims,
                    expires: expiresAt.UtcDateTime,
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