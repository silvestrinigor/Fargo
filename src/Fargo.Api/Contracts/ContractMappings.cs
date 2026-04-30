using Fargo.Application.ApiClients;
using Fargo.Application.Authentication;

namespace Fargo.Sdk.Contracts;

internal static class ContractMappings
{
    public static Fargo.Sdk.Contracts.Authentication.AuthResult ToContract(this AuthResult result)
        => new(
            result.AccessToken.Value,
            result.RefreshToken.Value,
            result.ExpiresAt,
            result.IsAdmin,
            result.PermissionActions.Select(static action => (Fargo.Sdk.Contracts.ActionType)(int)action).ToArray(),
            result.PartitionAccesses.ToArray());

    public static Fargo.Sdk.Contracts.ApiClients.ApiClientCreatedResult ToContract(this Fargo.Application.ApiClients.ApiClientCreatedResult result)
        => new(result.Guid, result.PlainKey);
}
