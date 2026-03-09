namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Provides helper extensions for accessing configuration values.
    /// </summary>
    /// <remarks>
    /// This extension centralizes configuration access patterns used by the HTTP API.
    /// In particular, it simplifies retrieval of values from the JWT configuration section.
    /// </remarks>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// The root configuration section used for JWT settings.
        /// </summary>
        public const string JwtConfiguration = "Jwt";

        extension(IConfiguration configuration)
        {
            /// <summary>
            /// Retrieves a value from the JWT configuration section.
            /// </summary>
            /// <param name="value">
            /// The key inside the <c>Jwt</c> configuration section.
            /// </param>
            /// <returns>
            /// The configuration value if found; otherwise <see langword="null"/>.
            /// </returns>
            /// <remarks>
            /// This helper is intended to simplify access to JWT-related configuration
            /// values defined under the <c>Jwt</c> section of the application configuration.
            ///
            /// Example configuration structure:
            /// <code>
            /// "Jwt": {
            ///   "Issuer": "...",
            ///   "Audience": "...",
            ///   "Secret": "..."
            /// }
            /// </code>
            /// </remarks>
            public string? GetJwtConfiguration(string value)
            {
                return configuration[$"{JwtConfiguration}:{value}"];
            }
        }
    }
}