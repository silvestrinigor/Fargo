using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Dtos.ServiceAccounts;
using Fargo.Application.Queries.ServiceAccounts;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class ServiceAccountServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoServiceAccountApplication() => services
        .AddFargoCommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto, ServiceAccountCreateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountUpdateCommand, ServiceAccountUpdateCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountSetActiveCommand, ServiceAccountSetActiveCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountSetPermissionsCommand, ServiceAccountSetPermissionsCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto, ServiceAccountRotateSecretCommandHandler>()
        .AddFargoCommandHandler<ServiceAccountDeleteCommand, ServiceAccountDeleteCommandHandler>()
        .AddFargoQueryHandler<ServiceAccountSingleQuery, ServiceAccountDto?, ServiceAccountSingleQueryHandler>()
        .AddFargoQueryHandler<ServiceAccountsQuery, PagedResult<ServiceAccountDto>, ServiceAccountsQueryHandler>();
    }
}
