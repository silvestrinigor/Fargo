namespace Fargo.SeedService.Extensions
{
    public static class ConfigurationExtension
    {
        public const string ApplicationConfiguration = "Application";

        extension(IConfiguration configuration)
        {
            public string? GetApplicationConfiguration(string value)
            {
                return configuration[$"{ApplicationConfiguration}:{value}"];
            }
        }
    }
}