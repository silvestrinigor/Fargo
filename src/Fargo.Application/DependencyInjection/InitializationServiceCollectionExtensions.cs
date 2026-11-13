using Fargo.Application.Commands.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class InitializationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInitializationApplication() => services
        .AddFargoCommandHandler<InitializeSystemCommand, InitializeSystemCommandHandler>();
    }
}
