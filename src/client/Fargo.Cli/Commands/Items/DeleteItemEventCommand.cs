using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class DeleteItemEventCommand : Command
{
    public DeleteItemEventCommand(FargoApiClient client)
        : base("delete", """
            Record a deletion event: a quantity of an event-tracked article that
            left stock, optionally from a container.

            Event-based stock is not unit-tracked, so there is no item GUID to
            delete; you describe how much left and from where. Prints the recorded
            event's GUID.

            Examples:
              fargo item event delete --article 11111111-1111-1111-1111-111111111111 --quantity 10
              fargo item event delete --article 11111111-1111-1111-1111-111111111111 --quantity 10 --container 22222222-2222-2222-2222-222222222222
            """)
    {
        var articleOption = new Option<Guid?>("--article")
        {
            Description = "GUID of the event-tracked article.",
            Required = true
        };

        var quantityOption = new Option<double?>("--quantity")
        {
            Description = "The strictly positive quantity deleted.",
            Required = true
        };

        var containerOption = new Option<Guid?>("--container")
        {
            Description = "GUID of the container the quantity was deleted from. Omit for outside any container."
        };

        var atOption = new Option<DateTimeOffset?>("--at")
        {
            Description = "Instant (ISO 8601 UTC) at which the deletion occurred. Defaults to now."
        };

        Add(articleOption);
        Add(quantityOption);
        Add(containerOption);
        Add(atOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var article = parseResult.GetValue(articleOption);
            var quantity = parseResult.GetValue(quantityOption);
            var container = parseResult.GetValue(containerOption);
            var at = parseResult.GetValue(atOption);

            var dto = new ItemEventCreateDto
            {
                EventType = (int)ItemEventType.Deletion,
                ArticleGuid = article,
                Quantity = new Scalar { Double = quantity },
                ItemContainerGuid = container,
                OccuredAt = at
            };

            var guid = await client.V1.Item.Events.PostAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Item deletion event recorded.",
                ("Event", guid?.ToString()),
                ("Article", article?.ToString()),
                ("Quantity", quantity?.ToString()),
                ("Container", container?.ToString() ?? "(outside)"));

            return 0;
        });
    }
}
