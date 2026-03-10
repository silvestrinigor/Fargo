namespace Fargo.Infrastructure.Security
{
    /// <summary>
    /// Configuration options used for generating JSON Web Tokens (JWT).
    /// </summary>
    /// <remarks>
    /// These settings are typically bound from application configuration
    /// (for example <c>appsettings.json</c> or environment variables)
    /// under the <c>Jwt</c> section.
    ///
    /// The values defined here control how access tokens are created,
    /// including the signing key, issuer, audience, and expiration time.
    /// </remarks>
    public sealed class JwtOptions
    {
        /// <summary>
        /// The configuration section name used for binding these options.
        /// </summary>
        public const string SectionName = "Jwt";

        /// <summary>
        /// Gets the secret key used to sign JWT tokens.
        /// </summary>
        /// <remarks>
        /// This key is used to create the cryptographic signature of the token.
        /// It should be kept secure and must be sufficiently long to ensure
        /// strong security for the chosen signing algorithm.
        /// </remarks>
        public required string Key { get; init; }

        /// <summary>
        /// Gets the issuer of the JWT token.
        /// </summary>
        /// <remarks>
        /// The issuer identifies the system that generated the token
        /// and is typically validated by the resource server when
        /// processing authenticated requests.
        /// </remarks>
        public required string Issuer { get; init; }

        /// <summary>
        /// Gets the intended audience of the JWT token.
        /// </summary>
        /// <remarks>
        /// The audience identifies the recipients that the token is intended for.
        /// APIs validating the token should ensure the audience matches
        /// their expected value.
        /// </remarks>
        public required string Audience { get; init; }

        /// <summary>
        /// Gets the lifetime of generated access tokens in minutes.
        /// </summary>
        /// <remarks>
        /// The default value is 120 minutes (2 hours).
        /// This value determines the expiration time embedded in the token.
        /// </remarks>
        public int AccessTokenExpirationInMinutes { get; init; } = 120;
    }
}