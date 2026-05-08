using Fargo.Application.Authentication;

namespace Fargo.SeedService.Extensions;

/// <summary>
/// Provides extension methods for configuring the worker.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers and validates the configuration used to create the
        /// default administrator account during system initialization.
        /// </summary>
        /// <param name="configuration">
        /// The application configuration instance used to bind the
        /// default administrator settings.
        /// </param>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance to allow
        /// method chaining.
        /// </returns>
        /// <remarks>
        /// This method binds the <see cref="DefaultAdminOptions"/> configuration
        /// from the <c>DefaultAdmin</c> section of the application configuration.
        ///
        /// The configuration is validated during application startup to ensure
        /// that the required administrator credentials are provided.
        /// The validation process guarantees that both the administrator
        /// identifier and password are present before the system initialization
        /// process executes.
        ///
        /// The use of <c>ValidateOnStart()</c> ensures that the application fails
        /// fast if the configuration is missing or invalid.
        /// </remarks>
        public IServiceCollection AddFargoDefaultAdmin(
                IConfiguration configuration
                )
        {
            services
                .AddOptions<DefaultAdminOptions>()
                .Bind(configuration.GetSection(DefaultAdminOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Nameid),
                        "DefaultAdmin:Nameid must be provided.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Password),
                        "DefaultAdmin:Password must be provided.")
                .ValidateOnStart();

            return services;
        }
    }
}
