using Fargo.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Fargo.Application.Commands.Initialization;

namespace Fargo.Application.DependencyInjection;

public static class InitializationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInitializationApplication() => services
        .AddFargoCommandHandler<InitializeSystemCommand, InitializeSystemCommandHandler>();
    }
}
