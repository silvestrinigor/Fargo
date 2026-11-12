using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.ServiceAccounts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.ServiceAccounts;

/// <summary>
/// Registers the ServiceAccounts use-case handlers.
/// </summary>
public static class ServiceAccountServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoServiceAccountApplication() => services
        .AddFargoCommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto, ServiceAccountCreateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountUpdateCommand, ServiceAccountUpdateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto, ServiceAccountRotateSecretCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountDeleteCommand, ServiceAccountDeleteCommandHandler>()
        .AddFargoQueryHandler<ServiceAccountSingleQuery, ServiceAccountDto?, ServiceAccountSingleQueryHandler>()
        .AddFargoQueryHandler<ServiceAccountsQuery, PagedResult<ServiceAccountDto>, ServiceAccountsQueryHandler>();
    }
}
