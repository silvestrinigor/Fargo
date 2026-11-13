using Fargo.Application.Commands.Users;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Users;
using Fargo.Application.Queries.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class UserServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoUserApplication() => services
        .AddFargoCommandHandler<UserCreateCommand, Guid, UserCreateCommandHandler>()
        .AddFargoCommandHandler<UserUpdateCommand, UserUpdateCommandHandler>()
        .AddFargoCommandHandler<UserSetActiveCommand, UserSetActiveCommandHandler>()
        .AddFargoCommandHandler<UserSetPermissionsCommand, UserSetPermissionsCommandHandler>()
        .AddFargoCommandHandler<UserSetUserGroupsCommand, UserSetUserGroupsCommandHandler>()
        .AddFargoCommandHandler<UserSetPartitionsCommand, UserSetPartitionsCommandHandler>()
        .AddFargoCommandHandler<UserResetPasswordCommand, UserResetPasswordCommandHandler>()
        .AddFargoCommandHandler<UserSetPasswordExpirationCommand, UserSetPasswordExpirationCommandHandler>()
        .AddFargoCommandHandler<UserRemovePasswordExpirationCommand, UserRemovePasswordExpirationCommandHandler>()
        .AddFargoCommandHandler<UserDeleteCommand, UserDeleteCommandHandler>()
        .AddFargoQueryHandler<UserSingleQuery, UserDto?, UserSingleQueryHandler>()
        .AddFargoQueryHandler<UsersQuery, PagedResult<UserDto>, UsersQueryHandler>();
    }
}
