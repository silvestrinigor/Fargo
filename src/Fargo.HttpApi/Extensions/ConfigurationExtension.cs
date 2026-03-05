namespace Fargo.HttpApi.Extensions
{
    public static class ConfigurationExtension
    {
        public const string ApplicationConfiguration = "Application";

        public const string JwtConfiguration = "Jwt";

        extension(IConfiguration configuration)
        {
            public string? GetApplicationConfiguration(string value)
            {
                return configuration[$"{ApplicationConfiguration}:{value}"];
            }

            public string? GetJwtConfiguration(string value)
            {
                return configuration[$"{JwtConfiguration}:{value}"];
            }
        }
    }
}