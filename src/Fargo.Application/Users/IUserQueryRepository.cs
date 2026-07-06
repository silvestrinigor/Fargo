using Fargo.Application.Shared.Users;

namespace Fargo.Application.Users;

public interface IUserQueryRepository
{
    Task<UserDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserDto>> GetManyInfoAsync(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);
}
