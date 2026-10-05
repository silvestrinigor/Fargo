using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class ListUsersCommand : Command
{
    public ListUsersCommand(FargoApiClient client)
        : base("list", "List users")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of users per page."
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
                    request.QueryParameters.ChildOfAnyOfThesePartitions = partitions.Select(g => (Guid?)g).ToArray();
                }
            });

            foreach (var user in result ?? [])
            {
                Console.WriteLine($"{user.Guid}  {user.Nameid}  {user.FirstName} {user.LastName}");
            }
        });
    }
}
