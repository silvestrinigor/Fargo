using Fargo.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Initialization;

/// <summary>
/// Registers the Initialization use-case handlers.
/// </summary>
public static class InitializationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoInitializationApplication() => services
        .AddFargoCommandHandler<InitializeSystemCommand, InitializeSystemCommandHandler>();
    }
}
