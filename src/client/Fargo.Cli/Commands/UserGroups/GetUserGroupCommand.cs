using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class GetUserGroupCommand : Command
{
    public GetUserGroupCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single user group.

            Examples:
              fargo user-group get 00000000-0000-0000-0000-000000000003
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var group = await client.UserGroups[guid].GetAsync(cancellationToken: cancellationToken);

            if (group is null)
            {
                Console.Error.WriteLine("User group not found.");
                return 1;
            }

            new DetailWriter()
                .Title("User group")
                .Field("GUID", group.Guid)
                .Field("Name", group.Nameid)
                .Field("Description", group.Description)
                .Field("Active", group.IsActive)
                .Field("Admin group", group.IsAdminUserGroup)
                .Field("Parent user group", group.ParentUserGroup)
                .Field("Permissions", FormatPermissions(group.Permissions))
                .Field("Partition accesses", FormatGuids(group.PartitionAccesses))
                .Field("Partitions", FormatGuids(group.Partitions))
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
