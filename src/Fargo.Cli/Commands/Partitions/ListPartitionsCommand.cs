using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class ListPartitionsCommand : Command
{
    public ListPartitionsCommand(FargoApiClient client)
        : base("list", "List partitions")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of partitions per page."
        };

        var parentOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Filter by parent partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(parentOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(parentOption);

            var result = await client.Partitions.GetAsync(request =>
            {
                if (page.HasValue) request.QueryParameters.Page = page.Value;
                if (limit.HasValue) request.QueryParameters.Limit = limit.Value;
                if (partitions is { Length: > 0 })
                    request.QueryParameters.ChildOfAnyOfThesePartitions = partitions.Select(g => (Guid?)g).ToArray();
            });

            foreach (var partition in result ?? [])
            {
                Console.WriteLine($"{partition.Guid}  {partition.Name}");
            }
        });
    }
}
