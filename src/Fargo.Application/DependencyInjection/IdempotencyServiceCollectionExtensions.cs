using Fargo.Application.Commands.Idempotency;
using Fargo.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class IdempotencyServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// The decorator reads <see cref="IdempotencyOptions"/>; a host that binds the <c>Idempotency</c>
        /// section overrides the defaults.
        /// </summary>
        public IServiceCollection AddFargoIdempotencyApplication()
        {
            services.AddOptions<IdempotencyOptions>();
            services.AddScoped<IdempotencyOutcome>();
            services.AddScoped<IdempotencyGate>();

            return services.AddFargoCommandHandler<
                PurgeExpiredIdempotencyKeysCommand, int, PurgeExpiredIdempotencyKeysCommandHandler>();
        }
    }
}
