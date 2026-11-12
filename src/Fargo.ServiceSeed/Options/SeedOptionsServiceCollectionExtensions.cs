namespace Fargo.ServiceSeed.Options;

public static class SeedOptionsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Binds and validates the seed options (<c>DefaultAdmin</c>, administrators group, global partition).
        /// <c>ValidateOnStart()</c> makes the host fail fast when any of them is missing or invalid.
        /// </summary>
        public IServiceCollection AddFargoSeedOptions(IConfiguration configuration)
        {
            services
            .AddOptions<DefaultAdminOptions>()
            .Bind(configuration.GetSection(DefaultAdminOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

            services
            .AddOptions<AdministratorsUserGroupOptions>()
            .Bind(configuration.GetSection(AdministratorsUserGroupOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

            services
            .AddOptions<GlobalPartitionOptions>()
            .Bind(configuration.GetSection(GlobalPartitionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

            return services;
        }
    }
}
