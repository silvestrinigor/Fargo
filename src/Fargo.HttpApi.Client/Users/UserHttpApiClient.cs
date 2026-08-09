using Fargo.Application;
using Fargo.Application.Shared.Users;

namespace Fargo.HttpApi.Client.Users;

public sealed class UserHttpApiClient : IUserHttpApiClient
{
    public Task<Guid> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto?> GetAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<UserDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid userGuid, UserUpdateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
