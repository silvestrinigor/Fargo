using Fargo.Application.Common;

namespace Fargo.Application.Users;

public interface IUserQueryRepository
{
    Task<UserDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
