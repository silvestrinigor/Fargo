using Fargo.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Initialization;

public static class InitializationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInitializationApplication() => services
        .AddFargoCommandHandler<InitializeSystemCommand, InitializeSystemCommandHandler>();
    }
}
