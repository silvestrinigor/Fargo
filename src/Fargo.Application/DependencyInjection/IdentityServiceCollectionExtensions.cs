using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.Identity;
using Fargo.Application.Dtos.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

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
