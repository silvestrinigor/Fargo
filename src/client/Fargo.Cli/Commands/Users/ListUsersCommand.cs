using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class ListUsersCommand : Command
{
    public ListUsersCommand(FargoApiClient client)
        : base("list", """
            List users as a table, with optional paging and partition filters.

            Examples:
              fargo user list
              fargo user list --page 1 --limit 20
              fargo user list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of users per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show users under these partition GUIDs (space-separated).",
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

            var result = await client.Users.GetAsync(request =>
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

            new TableWriter<Fargo.ClientHttp.Models.UserDto>()
                .Column("GUID", u => u.Guid?.ToString())
                .Column("USERNAME", u => u.Nameid)
                .Column("NAME", u => $"{u.FirstName} {u.LastName}".Trim())
                .Column("ACTIVE", u => u.IsActive?.ToString())
                .Column("ADMIN", u => u.IsAdmin?.ToString())
                .EmptyMessage("No users found.")
                .Write(result);

            return 0;
        });
    }
}
