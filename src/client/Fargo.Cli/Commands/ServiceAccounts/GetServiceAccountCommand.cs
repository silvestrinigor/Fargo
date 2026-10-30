using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class GetServiceAccountCommand : Command
{
    public GetServiceAccountCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single service account. Never shows the secret.

            Examples:
              fargo service-account get 00000000-0000-0000-0000-000000000010
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var account = await client.V1.ServiceAccounts[guid].GetAsync(cancellationToken: cancellationToken);

            if (account is null)
            {
                Console.Error.WriteLine("Service account not found.");
                return 1;
            }

            new DetailWriter()
                .Title("Service account")
                .Field("GUID", account.Guid)
                .Field("Client ID", account.ClientId)
                .Field("Description", account.Description)
                .Field("Active", account.IsActive)
                .Field("Permissions", FormatPermissions(account.Permissions))
                .Field("Partition accesses", FormatGuids(account.PartitionAccessGuids))
                .Write();

            return 0;
        });
    }

    private static string? FormatGuids(IEnumerable<Guid?>? guids)
    {
        if (guids is null)
        {
            return null;
        }

        var values = guids.Where(g => g.HasValue).Select(g => g!.Value.ToString()).ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }

    private static string? FormatPermissions(IEnumerable<int?>? permissions)
    {
        if (permissions is null)
        {
            return null;
        }

        var values = permissions
            .Where(p => p.HasValue)
            .Select(p => EnumFormatting.Format<ActionType>(p) ?? $"unknown ({p!.Value})")
            .ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }
}