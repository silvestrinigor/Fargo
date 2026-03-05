using Fargo.Application.Requests.Commands;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Extensions
{
    public static class ServiceProviderExtension
    {
        extension(IServiceProvider services)
        {
            public async Task<IServiceProvider> InitializeSystem(
                    string? defaultAdminNameid,
                    string? defaultAdminPassword
                    )
            {
                using (var scope = services.CreateScope())
                {
                    var initializeSystem = scope.ServiceProvider
                        .GetRequiredService<ICommandHandler<InitializeSystemCommand>>();

                    var initializeSystemCommand = new InitializeSystemCommand(
                            defaultAdminNameid != null ? new Nameid(defaultAdminNameid) : null,
                            defaultAdminPassword != null ? new Password(defaultAdminPassword) : null
                            );

                    await initializeSystem.Handle(initializeSystemCommand);
                }

                return services;
            }
        }
    }
}