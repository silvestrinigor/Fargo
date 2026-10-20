using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class ListUserGroupsCommand : Command
{
    public ListUserGroupsCommand(FargoApiClient client)
        : base("list", """
            List user groups as a table, with optional paging and partition filters.

            Examples:
              fargo user-groups list
              fargo user-groups list --page 1 --limit 20
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of user groups per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show user groups under these partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(partitionsOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var result = await client.UserGroups.GetAsync(request =>
            {
                if (page.HasValue)
                {
                    request.QueryParameters.Page = page.Value;
                }

                if (limit.HasValue)
                {
                    request.QueryParameters.Limit = limit.Value;
                }

                if (partitions is { Length: > 0 })
                {
                    request.QueryParameters.ChildOfAnyOfThesePartitions =
                        partitions.Select(g => (Guid?)g).ToArray();
                }
            });

            new TableWriter<Fargo.ClientHttp.Models.UserGroupDto>()
                .Column("GUID", g => g.Guid?.ToString())
                .Column("NAME", g => g.Nameid)
                .Column("ACTIVE", g => g.IsActive?.ToString())
                .Column("ADMIN", g => g.IsAdminUserGroup?.ToString())
                .EmptyMessage("No user groups found.")
                .Write(result);

            return 0;
        });
    }
}
