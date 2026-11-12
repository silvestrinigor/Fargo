using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.Users;
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
        .AddFargoCommandHandler<UserDeleteCommand, UserDeleteCommandHandler>()
        .AddFargoQueryHandler<UserSingleQuery, UserDto?, UserSingleQueryHandler>()
        .AddFargoQueryHandler<UsersQuery, PagedResult<UserDto>, UsersQueryHandler>();
    }
}
