using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Identity.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Registers the Identity use-case handlers.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoIdentityApplication() => services
        .AddFargoCommandHandler<IdentityLoginCommand, IdentityAuthResultDto, IdentityLoginCommandHandler>()
        .AddFargoCommandHandler<IdentityLogoutCommand, IdentityLogoutCommandHandler>()
        .AddFargoCommandHandler<IdentityRefreshCommand, IdentityAuthResultDto, IdentityRefreshCommandHandler>()
        .AddFargoCommandHandler<IdentityPasswordChangeCommand, IdentityPasswordChangeCommandHandler>()
        .AddFargoCommandHandler<ServiceTokenCommand, ServiceTokenResultDto, ServiceTokenCommandHandler>();
    }
}
