using Fargo.Application;
using Fargo.Application.Shared.Users;

namespace Fargo.HttpApi.Client.Users;

public interface IUserHttpApiClient
{
    Task<UserDto?> GetAsync(Guid userGuid, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid userGuid, UserUpdateDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
