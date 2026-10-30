using Fargo.Core.Identity;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

public static class ServiceAccountMappings
{
    public static ServiceAccountDto ToDto(this ServiceAccount account) => new(
        account.Guid,
        account.ClientId.ToString(),
        account.Description.ToString(),
        account.IsActive,
        [.. account.Permissions],
        [.. account.PartitionAccessGuids]);
}