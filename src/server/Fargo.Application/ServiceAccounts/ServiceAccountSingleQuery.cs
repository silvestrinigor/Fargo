using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Query for a single service account by id.</summary>
public sealed record ServiceAccountSingleQuery(
    Guid ServiceAccountGuid
) : IQuery<ServiceAccountDto?>;