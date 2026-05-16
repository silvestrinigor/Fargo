using Fargo.Application;
using Fargo.Application.Items;
using Fargo.HttpApi.Contracts;
using Fargo.Sdk.Contracts.Items;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Items;

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
            .Produces<ItemInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<ItemInfo>, NotFound>> GetSingleItem(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ItemSingleQuery, ItemDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new ItemSingleQuery(itemGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToInfo());
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetItems(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyItem)
            .WithName("GetItems")
            .WithSummary("Gets multiple items")
            .WithDescription("Retrieves a paginated list of items. Supports optional temporal queries and partition filters, including public items without partitions.")
            .Produces<IReadOnlyCollection<ItemInfo>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemInfo>>, NoContent>> GetManyItem(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        [FromQuery] Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ItemsQuery(
            withPagination,
            temporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response.ToInfo());
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
        ItemCreateRequest request,
        ItemApplicationService service,
        CancellationToken cancellationToken)
    {
        var response = await service.Create(request.ToApplicationDto(), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateItem(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{itemGuid:guid}", UpdateItem)
            .WithName("UpdateItem")
            .WithSummary("Replaces an existing item")
            .WithDescription("Replaces mutable item state including partitions and parent container.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateItem(
        Guid itemGuid,
        ItemUpdateRequest request,
        ItemApplicationService service,
        CancellationToken cancellationToken)
    {
        await service.Update(itemGuid, request.ToApplicationDto(), cancellationToken);

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
        ItemApplicationService service,
        CancellationToken cancellationToken)
    {
        await service.Delete(itemGuid, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
