namespace Fargo.ServiceInitializer.Options;

public static class InitializerOptionsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Binds and validates the initializer options (<c>DefaultAdmin</c>, administrators group, global partition).
        /// <c>ValidateOnStart()</c> makes the host fail fast when any of them is missing or invalid.
        /// </summary>
        public IServiceCollection AddFargoInitializerOptions(IConfiguration configuration)
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
