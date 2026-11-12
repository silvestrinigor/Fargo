using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Users;

namespace Fargo.Application.Persistence.Repositories;

public interface IUserQueryRepository
{
    Task<UserDto?> GetInfoByGuidAsync(
        Guid userGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Ordered by <paramref name="sort"/>, or by nameid then guid when it is null.
    /// </summary>
    /// <param name="search">Case-insensitive text matched against the user nameid.</param>
    Task<PagedResult<UserDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}
