using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
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
            accesses are partition GUIDs (space-separated).

            Examples:
              fargo service-account create --client-id erp-importer
              fargo service-account create --client-id erp-importer --permissions 1 4 6 --partitions 00000000-0000-0000-0000-000000000002
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
            Description = "ActionType permission codes to grant (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to grant access to (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(clientIdOption);
        Add(descriptionOption);
        Add(isActiveOption);
        Add(permissionsOption);
        Add(partitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var clientId = parseResult.GetValue(clientIdOption)!;
            var description = parseResult.GetValue(descriptionOption);
            var isActive = parseResult.GetValue(isActiveOption);
            var permissions = parseResult.GetValue(permissionsOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var dto = new ServiceAccountCreateDto
            {
                ClientId = clientId,
                Description = description,
                IsActive = isActive,
                PermissionsToAdd = permissions is { Length: > 0 }
                    ? permissions.Select(p => (int?)p).ToList()
                    : null,
                PartitionAccessesToAdd = partitions is { Length: > 0 }
                    ? partitions.Select(g => (Guid?)g).ToList()
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