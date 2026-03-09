namespace Fargo.SeedService.Extensions
{
    /// <summary>
    /// Provides extension members for accessing application-specific configuration values.
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Gets the root configuration section name used for application settings.
        /// </summary>
        public const string ApplicationConfiguration = "Application";

        /// <summary>
        /// Adds helper members to <see cref="IConfiguration"/> for reading values
        /// from the application configuration section.
        /// </summary>
        /// <param name="configuration">
        /// The configuration instance used to access application settings.
        /// </param>
        extension(IConfiguration configuration)
        {
            /// <summary>
            /// Gets a configuration value from the <see cref="ApplicationConfiguration"/> section.
            /// </summary>
            /// <param name="value">
            /// The key of the configuration value inside the application section.
            /// </param>
            /// <returns>
            /// The configuration value associated with the specified key,
            /// or <see langword="null"/> if the key does not exist.
            /// </returns>
            public string? GetApplicationConfiguration(string value)
            {
                return configuration[$"{ApplicationConfiguration}:{value}"];
            }
        }
    }
}