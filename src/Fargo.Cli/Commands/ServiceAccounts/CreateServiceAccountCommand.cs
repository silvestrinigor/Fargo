using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class CreateServiceAccountCommand : Command
{
    public CreateServiceAccountCommand(FargoApiClient client)
        : base("create", """
            Create a new service account. The generated client secret is printed
            EXACTLY ONCE - store it securely, it cannot be retrieved later (only
            rotated).

            Permissions are numeric ActionType codes (space-separated). Partition
            memberships and partition accesses are partition GUIDs (space-separated).
            Membership defines the partitions the account belongs to (its scope);
            access defines the partitions the account is allowed to reach.

            Examples:
              fargo service-account create --client-id erp-importer
              fargo service-account create --client-id erp-importer --permissions 1 4 6 --partition-memberships 00000000-0000-0000-0000-000000000002 --partition-accesses 00000000-0000-0000-0000-000000000002
            """)
    {
        var clientIdOption = new Option<string>("--client-id")
        {
            Description = "Unique client identifier (nameid format) for the service account.",
            Required = true
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "Optional human-readable description."
        };

        var isActiveOption = new Option<bool>("--active")
        {
            Description = "Whether the account is active. Defaults to true.",
            DefaultValueFactory = _ => true
        };

        var permissionsOption = new Option<int[]?>("--permissions")
        {
            Description = "ActionType permission codes to grant (space-separated). Requires the ChangeServiceAccountPermissions permission.",
            AllowMultipleArgumentsPerToken = true
        };

        var partitionMembershipsOption = new Option<Guid[]?>("--partition-memberships")
        {
            Description = "Partition GUIDs the account belongs to - its partition membership (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var partitionAccessesOption = new Option<Guid[]?>("--partition-accesses", "--partitions")
        {
            Description = "Partition GUIDs to grant access to (space-separated). Alias: --partitions.",
            AllowMultipleArgumentsPerToken = true
        };

        Add(clientIdOption);
        Add(descriptionOption);
        Add(isActiveOption);
        Add(permissionsOption);
        Add(partitionMembershipsOption);
        Add(partitionAccessesOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var clientId = parseResult.GetValue(clientIdOption)!;
            var description = parseResult.GetValue(descriptionOption);
            var isActive = parseResult.GetValue(isActiveOption);
            var permissions = parseResult.GetValue(permissionsOption);
            var partitionMemberships = parseResult.GetValue(partitionMembershipsOption);
            var partitionAccesses = parseResult.GetValue(partitionAccessesOption);

            var dto = new ServiceAccountCreateDto
            {
                ClientId = clientId,
                Description = description,
                IsActive = isActive,
                PermissionsToAdd = permissions is { Length: > 0 }
                    ? [.. permissions.Select(p => (int?)p)]
                    : null,
                PartitionsToAdd = partitionMemberships is { Length: > 0 }
                    ? [.. partitionMemberships.Select(g => (Guid?)g)]
                    : null,
                PartitionAccessesToAdd = partitionAccesses is { Length: > 0 }
                    ? [.. partitionAccesses.Select(g => (Guid?)g)]
                    : null
            };

            var created = await client.V1.ServiceAccounts.PostAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Service account created. Store the secret now - it will not be shown again.",
                ("GUID", created?.Guid?.ToString()),
                ("Client ID", created?.ClientId),
                ("Client secret", created?.ClientSecret));

            return 0;
        });
    }
}
