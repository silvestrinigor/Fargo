using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class ListUserGroupsCommand : Command
{
    public ListUserGroupsCommand(FargoApiClient client)
        : base("list", "List user groups")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of user groups per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Filter by partition GUIDs (space-separated).",
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
                    request.QueryParameters.ChildOfAnyOfThesePartitions = partitions.Select(g => (Guid?)g).ToArray();
                }
            });

            foreach (var group in result ?? [])
            {
                Console.WriteLine($"{group.Guid}  {group.Nameid}  active={group.IsActive}");
            }
        });
    }
}
