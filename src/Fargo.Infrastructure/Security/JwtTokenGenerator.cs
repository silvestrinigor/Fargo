using Fargo.Application.Security;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Signs tokens with HMAC SHA-256. Besides the identifier, name and subject claims, each token carries the
/// actor's auth version, so tokens can be revoked.
/// </summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ITokenGenerator
{
    private readonly JwtOptions options = jwtOptions.Value;

    public TokenGenerateResult Generate(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Key));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ActorGuid.ToString()),
            new(ClaimTypes.Name, user.Actor.Nameid.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, user.ActorGuid.ToString()),
            new(FargoJwtClaims.AuthVersion, user.Authentication.AuthVersion.ToString()),
        };

        var expiresAt = timeProvider.GetUtcNow().AddMinutes(
            options.AccessTokenExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenGenerateResult(new Token(tokenString), expiresAt);
    }

    /// <summary>
    /// Like a user token, but stamps the actor-type claim as "service".
    /// </summary>
    public TokenGenerateResult Generate(ServiceAccount serviceAccount)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Key));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, serviceAccount.ActorGuid.ToString()),
            new(ClaimTypes.Name, serviceAccount.Actor.Nameid.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, serviceAccount.ActorGuid.ToString()),
            new(FargoJwtClaims.AuthVersion, serviceAccount.AuthVersion.ToString()),
            new(FargoJwtClaims.ActorType, FargoJwtClaims.ActorTypeService),
        };

        var expiresAt = timeProvider.GetUtcNow().AddMinutes(
            options.AccessTokenExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenGenerateResult(new Token(tokenString), expiresAt);
    }
}
