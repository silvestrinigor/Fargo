using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Metadata;

public sealed class MetadataCommand : Command
{
    public MetadataCommand(FargoApiClient client)
        : base("metadata", "List the numeric codes and names the API uses for its enumerations.")
    {
        var m = client.V1.Metadata;

        Add(Catalog("entity-types", "List the entity types.", ct => m.EntityTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("audit-log-types", "List the audit log types.", ct => m.AuditLogTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("action-types", "List the action (permission) types.", ct => m.ActionTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("actor-types", "List the actor types.", ct => m.ActorTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("article-types", "List the article types.", ct => m.ArticleTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("article-tracking-modes", "List the article tracking modes.", ct => m.ArticleTrackingModes.GetAsync(cancellationToken: ct)));
        Add(Catalog("barcode-formats", "List the barcode formats.", ct => m.BarcodeFormats.GetAsync(cancellationToken: ct)));
        Add(Catalog("item-event-types", "List the item event types.", ct => m.ItemEventTypes.GetAsync(cancellationToken: ct)));
        Add(Catalog("supply-order-statuses", "List the supply order statuses.", ct => m.SupplyOrderStatuses.GetAsync(cancellationToken: ct)));
        Add(Catalog("error-types", "List the error types.", ct => m.ErrorTypes.GetAsync(cancellationToken: ct)));
    }

    private static Command Catalog(
        string name,
        string description,
        Func<CancellationToken, Task<List<CatalogEntryDto>?>> fetch)
    {
        var command = new Command(name, $"{description}\n\nExamples:\n  fargo metadata {name}");

        command.SetAction(async (_, cancellationToken) =>
        {
            var entries = await fetch(cancellationToken);

            new TableWriter<CatalogEntryDto>()
                .Column("ID", e => e.Id?.ToString())
                .Column("NAME", e => e.Name)
                .EmptyMessage("No entries found.")
                .Write(entries);

            return 0;
        });

        return command;
    }
}
