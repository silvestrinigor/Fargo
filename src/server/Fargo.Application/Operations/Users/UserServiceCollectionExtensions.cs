using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Users.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Users;

/// <summary>
/// Registers the Users use-case handlers.
/// </summary>
public static class UserServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoUserApplication() => services
        .AddFargoCommandHandler<UserCreateCommand, Guid, UserCreateCommandHandler>()
        .AddFargoCommandHandler<UserUpdateCommand, UserUpdateCommandHandler>()
        .AddFargoCommandHandler<UserDeleteCommand, UserDeleteCommandHandler>()
        .AddFargoQueryHandler<UserSingleQuery, UserDto?, UserSingleQueryHandler>()
        .AddFargoQueryHandler<UsersQuery, PagedResult<UserDto>, UsersQueryHandler>();
    }
}
