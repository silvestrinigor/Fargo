using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Generates JWT access tokens for authenticated users.
/// </summary>
/// <remarks>
/// This implementation creates a signed JWT using the configuration provided
/// through <see cref="JwtOptions"/>.
///
/// The generated token includes the following claims:
/// <list type="bullet">
/// <item>
/// <description><see cref="ClaimTypes.NameIdentifier"/> with the user identifier.</description>
/// </item>
/// <item>
/// <description><see cref="ClaimTypes.Name"/> with the user's nameid.</description>
/// </item>
/// <item>
/// <description><see cref="JwtRegisteredClaimNames.Sub"/> with the user identifier.</description>
/// </item>
/// </list>
///
/// The token is signed using HMAC SHA-256 and expires according to
/// <see cref="JwtOptions.AccessTokenExpirationInMinutes"/>.
/// </remarks>
public sealed class JwtTokenGenerator(
        IOptions<JwtOptions> jwtOptions
        ) : ITokenGenerator
{
    private readonly JwtOptions options = jwtOptions.Value;

    /// <summary>
    /// Generates a signed JWT for the specified user.
    /// </summary>
    /// <param name="user">
    /// The authenticated user for whom the token will be generated.
    /// </param>
    /// <returns>
    /// A <see cref="TokenGenerateResult"/> containing the serialized JWT
    /// and its expiration timestamp in UTC.
    /// </returns>
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
