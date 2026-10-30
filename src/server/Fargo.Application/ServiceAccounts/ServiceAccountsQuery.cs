using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Query for a page of service accounts.</summary>
public sealed record ServiceAccountsQuery(
    Pagination WithPagination
) : IQuery<PagedResult<ServiceAccountDto>>;