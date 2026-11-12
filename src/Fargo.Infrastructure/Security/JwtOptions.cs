using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 needs at least 256 bits (32 bytes), so a minimum of 32 characters is enforced: a missing or
    /// weak key fails validation at startup rather than producing insecure tokens.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "Jwt:Key must be at least 32 characters (256 bits) for HMAC-SHA256.")]
    public required string Key { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Issuer { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Audience { get; init; }

    [Range(1, int.MaxValue)]
    public int AccessTokenExpirationInMinutes { get; init; } = 120;
}
