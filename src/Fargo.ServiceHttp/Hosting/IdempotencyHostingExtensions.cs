using Fargo.Application.Idempotency;

namespace Fargo.ServiceHttp.Hosting;

public static class IdempotencyHostingExtensions
{
    /// <summary>
    /// Binds the <c>Idempotency</c> section and fails at startup when it is not usable.
    /// </summary>
    public static IServiceCollection AddFargoIdempotency(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<IdempotencyOptions>()
            .Bind(configuration.GetSection(IdempotencyOptions.SectionName))
            .Validate(options => options.Retention > TimeSpan.Zero, "Idempotency:Retention must be greater than zero.")
            .ValidateOnStart();

        return services;
    }
}