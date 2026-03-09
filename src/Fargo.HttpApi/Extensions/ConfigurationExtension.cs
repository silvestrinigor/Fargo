namespace Fargo.HttpApi.Extensions
{
    public static class ConfigurationExtension
    {
        public const string JwtConfiguration = "Jwt";

        extension(IConfiguration configuration)
        {
            public string? GetJwtConfiguration(string value)
            {
                return configuration[$"{JwtConfiguration}:{value}"];
            }
        }
    }
}