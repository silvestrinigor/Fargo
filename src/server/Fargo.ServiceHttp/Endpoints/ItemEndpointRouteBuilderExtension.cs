using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class ItemEndpointRouteBuilderExtension
{
    public static void MapFargoItem(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapItemGroup();

        group.MapGetItemByGuid();

        group.MapGetItemLocationByGuid();

        group.MapGetItemMovimentsByGuid();

        group.MapGetItemContainerInventoryByGuid();

        group.MapGetItems();

        group.MapCreateItem();

        group.MapUpdateItem();

        group.MapDeleteItem();
    }

    private static RouteGroupBuilder MapItemGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("Items", "/items")
        .RequireAuthorization()
        .WithTags("Items")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetItemByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{itemGuid:guid}", GetSingleItem)
            .WithName("GetItem")
            .WithSummary("Gets a single item")
            .WithDescription("Retrieves a single item by its unique identifier.")
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ItemDto>, NotFound>> GetSingleItem(
        Guid itemGuid,
        IQueryHandler<ItemSingleQuery, ItemDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ItemSingleQuery(itemGuid);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get location

    private static IEndpointRouteBuilder MapGetItemLocationByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{itemGuid:guid}/location", GetItemLocationByGuid)
            .WithName("GetItemLocation")
            .WithSummary("Gets the item location")
            .WithDescription("Retrieves the containment chain of an item: the item followed by each of its parent containers. The list is ordered from the item itself (first element) outward to its root container (last element). When 'at' is supplied, the chain is reconstructed from the containment history as it was at that instant.")
            .Produces<IReadOnlyCollection<ItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemDto>>, NotFound>> GetItemLocationByGuid(
        Guid itemGuid,
        DateTimeOffset? at,
        IQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemLocationQuery(itemGuid, at);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response.Count == 0 ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get moviments history

    private static IEndpointRouteBuilder MapGetItemMovimentsByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{itemGuid:guid}/moviments", GetItemMovimentsByGuid)
            .WithName("GetItemMoviments")
            .WithSummary("Gets the item moviments")
            .WithDescription("Retrieves a list of moviments that represents the moviment history of item.")
            .Produces<IReadOnlyCollection<ItemMovimentDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemMovimentDto>>, NotFound>> GetItemMovimentsByGuid(
        Guid itemGuid,
        IQueryHandler<ItemMovimentsQuery, IReadOnlyCollection<ItemMovimentDto>?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemMovimentsQuery(itemGuid);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get inventory

    private static IEndpointRouteBuilder MapGetItemContainerInventoryByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{itemGuid:guid}/inventory", GetItemContainerInventoryByGuid)
            .WithName("GetItemContainerInventory")
            .WithSummary("Gets the item container inventory")
            .WithDescription("Retrieves the per-article inventory contained in the item container. When 'at' is supplied, the inventory is reconstructed from the containment history as it was at that instant.")
            .Produces<IReadOnlyCollection<ItemContainerInventoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemContainerInventoryDto>>, NotFound>> GetItemContainerInventoryByGuid(
        Guid itemGuid,
        DateTimeOffset? at,
        IQueryHandler<ItemContainerInventoryByGuidQuery, IReadOnlyCollection<ItemContainerInventoryDto>?> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemContainerInventoryByGuidQuery(itemGuid, At: at);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetItems(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyItem)
            .WithName("GetItems")
            .WithSummary("Gets multiple items")
            .WithDescription("Retrieves a paginated list of items, including the total number of matching entries.")
            .Produces<PagedResult<ItemDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<ItemDto>>> GetManyItem(
        int? page,
        int? limit,
        Guid[]? childOfAnyOfThesePartitions,
        IQueryHandler<ItemsQuery, PagedResult<ItemDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var query = new ItemsQuery(
            withPagination,
            childOfAnyOfThesePartitions);

        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreateItem(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateItem)
            .WithName("CreateItem")
            .WithSummary("Creates a new item")
            .WithDescription("Creates a new item with optional partitions. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateItem(
        ItemCreateDto request,
        ICommandHandler<ItemCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new ItemCreateCommand(request);

        var response = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateItem(this IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/{itemGuid:guid}", UpdateItem)
            .WithName("UpdateItem")
            .WithSummary("Replaces an existing item")
            .WithDescription("Replaces mutable item state including partitions and parent container.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateItem(
        Guid itemGuid,
        ItemUpdateDto request,
        ICommandHandler<ItemUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new ItemUpdateCommand(itemGuid, request);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Delete

    private static IEndpointRouteBuilder MapDeleteItem(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{itemGuid:guid}", DeleteItem)
            .WithName("DeleteItem")
            .WithSummary("Deletes an item")
            .WithDescription("Deletes the specified item from the system.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeleteItem(
        Guid itemGuid,
        ICommandHandler<ItemDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new ItemDeleteCommand(itemGuid);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
