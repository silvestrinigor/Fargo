using Fargo.Application;
using Fargo.Application.Items;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.Api.Items;

public static class ItemEndpointRouteBuilderExtension
{
    public static void MapFargoItem(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapItemGroup();

        group.MapGetItem();

        group.MapGetItems();

        group.MapCreateItem();

        group.MapUpdateItem();

        group.MapDeleteItem();
    }

    private static RouteGroupBuilder MapItemGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/items")
            .RequireAuthorization()
            .WithTags("Items");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetItem(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{itemGuid:guid}", GetSingleItem)
            .WithName("GetItem")
            .WithSummary("Gets a single item")
            .WithDescription("Retrieves a single item by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<ItemDto>, NotFound>> GetSingleItem(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ItemSingleQuery, ItemDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ItemSingleQuery(itemGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetItems(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyItem)
            .WithName("GetItems")
            .WithSummary("Gets multiple items")
            .WithDescription("Retrieves a paginated list of items. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<ItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemDto>>, NoContent>> GetManyItem(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        [FromQuery] Guid[]? insideAnyOfThisPartitions,
        bool? notInsideAnyPartition,
        IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ItemsQuery(
            withPagination,
            temporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

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
        ItemCreateCommand request,
        ICommandHandler<ItemCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateItem(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{itemGuid:guid}", UpdateItem)
            .WithName("UpdateItem")
            .WithSummary("Replaces an existing item")
            .WithDescription("Replaces all item state including partitions.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateItem(
        Guid itemGuid,
        ItemUpdateDto request,
        ICommandHandler<ItemUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemUpdateCommand(itemGuid, request), cancellationToken);

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
        await handler.Handle(new ItemDeleteCommand(itemGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
