using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.UserGroups.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.UserGroups;

public static class UserGroupServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoUserGroupApplication() => services
        .AddFargoCommandHandler<UserGroupCreateCommand, Guid, UserGroupCreateCommandHandler>()
        .AddFargoCommandHandler<UserGroupUpdateCommand, UserGroupUpdateCommandHandler>()
        .AddFargoCommandHandler<UserGroupDeleteCommand, UserGroupDeleteCommandHandler>()
        .AddFargoQueryHandler<UserGroupSingleQuery, UserGroupDto?, UserGroupSingleQueryHandler>()
        .AddFargoQueryHandler<UserGroupsQuery, PagedResult<UserGroupDto>, UserGroupsQueryHandler>();
    }
}
