using Fargo.Api.Contracts;
using Fargo.Api.Contracts.Items;
using Fargo.Api.Contracts.Partitions;
using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension responsible for mapping all Item endpoints.
/// </summary>
public static class ItemEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all routes related to items.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoItem(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/items")
            .RequireAuthorization()
            .WithTags("Items");

        group.MapGet("/{itemGuid:guid}", GetSingleItem)
            .WithName("GetItem")
            .WithSummary("Gets a single item")
            .WithDescription("Retrieves a single item by its unique identifier. Supports querying historical data using temporal tables.")
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyItems)
            .WithName("GetItems")
            .WithSummary("Gets multiple items")
            .WithDescription("Retrieves a paginated list of items with optional filters such as parent item or article.")
            .Produces<IReadOnlyCollection<ItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateItem)
            .WithName("CreateItem")
            .WithSummary("Creates a new item")
            .WithDescription("Creates a new item and returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{itemGuid:guid}", UpdateItem)
            .WithName("UpdateItem")
            .WithSummary("Updates an existing item")
            .WithDescription("Updates an item using partial data.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{itemGuid:guid}", DeleteItem)
            .WithName("DeleteItem")
            .WithSummary("Deletes an item")
            .WithDescription("Deletes the specified item from the system.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{itemGuid:guid}/partitions", GetItemPartitions)
            .WithName("GetItemPartitions")
            .WithSummary("Gets the partitions containing an item")
            .WithDescription("Returns the partitions that directly contain the specified item.")
            .Produces<IReadOnlyCollection<PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{itemGuid:guid}/partitions/{partitionGuid:guid}", AddItemPartition)
            .WithName("AddItemPartition")
            .WithSummary("Adds a partition to an item")
            .WithDescription("Associates an existing partition with the specified item.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{itemGuid:guid}/partitions/{partitionGuid:guid}", RemoveItemPartition)
            .WithName("RemoveItemPartition")
            .WithSummary("Removes a partition from an item")
            .WithDescription("Removes the association between a partition and the specified item.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<ItemDto>, NotFound>> GetSingleItem(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ItemSingleQuery, ItemInformation?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemSingleQuery(itemGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemDto>>, NoContent>> GetManyItems(
        Guid? articleGuid,
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        Guid? partitionGuid,
        bool? noPartition,
        IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemManyQuery(
            articleGuid,
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            partitionGuid,
            noPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<ItemDto>>(response.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<Ok<Guid>> CreateItem(
        ItemCreateDto request,
        ICommandHandler<ItemCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateItem(
        Guid itemGuid,
        ItemUpdateDto request,
        ICommandHandler<ItemUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(itemGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteItem(
        Guid itemGuid,
        ICommandHandler<ItemDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new ItemDeleteCommand(itemGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionDto>>, NotFound, NoContent>> GetItemPartitions(
        Guid itemGuid,
        IQueryHandler<ItemPartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ItemPartitionsQuery(itemGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<PartitionDto>>(result.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<NoContent> AddItemPartition(
        Guid itemGuid,
        Guid partitionGuid,
        ICommandHandler<ItemAddPartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemAddPartitionCommand(itemGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveItemPartition(
        Guid itemGuid,
        Guid partitionGuid,
        ICommandHandler<ItemRemovePartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemRemovePartitionCommand(itemGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }
}
