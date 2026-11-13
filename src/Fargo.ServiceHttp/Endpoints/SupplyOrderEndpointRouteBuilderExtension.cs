using Fargo.Application.Abstractions;
using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Application.Idempotency;
using Fargo.Application.Queries.SupplyOrders;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UnitsNet;

namespace Fargo.ServiceHttp.Endpoints;

public static class SupplyOrderEndpointRouteBuilderExtension
{
    public static void MapFargoSupplyOrder(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapSupplyOrderGroup();

        group.MapGetSupplyOrderByGuid();
        group.MapGetSupplyOrders();
        group.MapCreateSupplyOrder();
        group.MapAddSupplyOrderLine();
        group.MapChangeSupplyOrderLineQuantity();
        group.MapRemoveSupplyOrderLine();
        group.MapSetSupplyOrderReceivingContainer();
        group.MapPlaceSupplyOrder();
        group.MapReceiveSupplyOrder();
        group.MapCancelSupplyOrder();
        group.MapCloseSupplyOrder();
    }

    private static RouteGroupBuilder MapSupplyOrderGroup(this IEndpointRouteBuilder builder)
        => builder
        .MapVersionedGroup("SupplyOrders", "/supply-orders")
        .RequireAuthorization()
        .WithTags("SupplyOrders")
        .RequireRateLimiting("general-authenticated");

    #region Get single

    private static IEndpointRouteBuilder MapGetSupplyOrderByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{supplyOrderGuid:guid}", GetSingleSupplyOrder)
            .WithName("GetSupplyOrder")
            .WithSummary("Gets a single supply order")
            .WithDescription("Retrieves a single supply order, including its lines and receiving container, by its unique identifier.")
            .Produces<SupplyOrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<SupplyOrderDto>, NotFound>> GetSingleSupplyOrder(
        Guid supplyOrderGuid,
        IQueryHandler<SupplyOrderSingleQuery, SupplyOrderDto?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new SupplyOrderSingleQuery(supplyOrderGuid), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get many

    private static IEndpointRouteBuilder MapGetSupplyOrders(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManySupplyOrders)
            .WithName("GetSupplyOrders")
            .WithSummary("Gets multiple supply orders")
            .WithDescription("Retrieves a paginated list of supply orders, optionally filtered by status and a case-insensitive search over the reference and description.")
            .Produces<PagedResult<SupplyOrderDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<SupplyOrderDto>>> GetManySupplyOrders(
        Page? page,
        Limit? limit,
        SupplyOrderStatus? status,
        string? search,
        IQueryHandler<SupplyOrdersQuery, PagedResult<SupplyOrderDto>> handler,
        CancellationToken cancellationToken)
    {
        var pagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new SupplyOrdersQuery(pagination, status, Search: search);

        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion

    #region Create

    private static IEndpointRouteBuilder MapCreateSupplyOrder(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateSupplyOrder)
            .WithName("CreateSupplyOrder")
            .WithSummary("Creates a supply order")
            .WithDescription("Creates a new draft supply order with optional lines, receiving container, and partitions. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateSupplyOrder(
        SupplyOrderCreateDto request,
        ICommandHandler<SupplyOrderCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new SupplyOrderCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion

    #region Lines

    public sealed record SupplyOrderLineQuantityRequest(Scalar OrderedQuantity);

    private static IEndpointRouteBuilder MapAddSupplyOrderLine(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{supplyOrderGuid:guid}/lines/{articleGuid:guid}", AddSupplyOrderLine)
            .WithName("AddSupplyOrderLine")
            .WithSummary("Adds a line to a draft supply order")
            .WithDescription("Adds a line for the given article and ordered quantity. Only valid while the order is a draft.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> AddSupplyOrderLine(
        Guid supplyOrderGuid,
        Guid articleGuid,
        SupplyOrderLineQuantityRequest request,
        ICommandHandler<SupplyOrderAddLineCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new SupplyOrderAddLineCommand(supplyOrderGuid, articleGuid, request.OrderedQuantity),
            cancellationToken);

        return TypedResults.NoContent();
    }

    private static IEndpointRouteBuilder MapChangeSupplyOrderLineQuantity(this IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/{supplyOrderGuid:guid}/lines/{articleGuid:guid}", ChangeSupplyOrderLineQuantity)
            .WithName("ChangeSupplyOrderLineQuantity")
            .WithSummary("Changes a draft supply order line quantity")
            .WithDescription("Changes the ordered quantity of a line. Only valid while the order is a draft.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> ChangeSupplyOrderLineQuantity(
        Guid supplyOrderGuid,
        Guid articleGuid,
        SupplyOrderLineQuantityRequest request,
        ICommandHandler<SupplyOrderChangeLineQuantityCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new SupplyOrderChangeLineQuantityCommand(supplyOrderGuid, articleGuid, request.OrderedQuantity),
            cancellationToken);

        return TypedResults.NoContent();
    }

    private static IEndpointRouteBuilder MapRemoveSupplyOrderLine(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{supplyOrderGuid:guid}/lines/{articleGuid:guid}", RemoveSupplyOrderLine)
            .WithName("RemoveSupplyOrderLine")
            .WithSummary("Removes a line from a draft supply order")
            .WithDescription("Removes the line for the given article. Only valid while the order is a draft.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> RemoveSupplyOrderLine(
        Guid supplyOrderGuid,
        Guid articleGuid,
        ICommandHandler<SupplyOrderRemoveLineCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new SupplyOrderRemoveLineCommand(supplyOrderGuid, articleGuid),
            cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion

    #region Receiving container

    public sealed record SupplyOrderReceivingContainerRequest(Guid? ReceivingContainerGuid);

    private static IEndpointRouteBuilder MapSetSupplyOrderReceivingContainer(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{supplyOrderGuid:guid}/receiving-container", SetSupplyOrderReceivingContainer)
            .WithName("SetSupplyOrderReceivingContainer")
            .WithSummary("Sets the receiving container of a draft supply order")
            .WithDescription("Sets or clears the container into which goods are received. A null container receives goods at the root. Only valid while the order is a draft.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> SetSupplyOrderReceivingContainer(
        Guid supplyOrderGuid,
        SupplyOrderReceivingContainerRequest request,
        ICommandHandler<SupplyOrderSetReceivingContainerCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new SupplyOrderSetReceivingContainerCommand(supplyOrderGuid, request.ReceivingContainerGuid),
            cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion

    #region Lifecycle

    private static IEndpointRouteBuilder MapPlaceSupplyOrder(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{supplyOrderGuid:guid}/place", PlaceSupplyOrder)
            .WithName("PlaceSupplyOrder")
            .WithSummary("Places a supply order")
            .WithDescription("Commits a draft supply order, freezing its lines. Requires at least one line.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> PlaceSupplyOrder(
        Guid supplyOrderGuid,
        ICommandHandler<SupplyOrderPlaceCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new SupplyOrderPlaceCommand(supplyOrderGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static IEndpointRouteBuilder MapReceiveSupplyOrder(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{supplyOrderGuid:guid}/receive", ReceiveSupplyOrder)
            .WithName("ReceiveSupplyOrder")
            .WithSummary("Receives goods against a supply order")
            .WithDescription("Receives the given quantities, effecting the stock change into the order's receiving container (or at the root when it has none) and advancing the order's received quantities and status. Over-receipt is permitted, and a received order accepts further receipts, so a retried request would be counted again. Send an Idempotency-Key header (a GUID) to make the request safe to retry: a retry with the same key does nothing and returns the first result. Use a new key for a different receipt.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesIdempotencyErrors();

        return builder;
    }

    private static async Task<NoContent> ReceiveSupplyOrder(
        Guid supplyOrderGuid,
        SupplyOrderReceiveDto request,
        [FromHeader(Name = IdempotencyEndpointExtensions.KeyHeaderName)] Guid? idempotencyKey,
        IdempotencyOutcome outcome,
        HttpContext httpContext,
        ICommandHandler<SupplyOrderReceiveCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new SupplyOrderReceiveCommand(supplyOrderGuid, request, idempotencyKey), cancellationToken);

        httpContext.Response.MarkIfReplayed(outcome);

        return TypedResults.NoContent();
    }

    private static IEndpointRouteBuilder MapCancelSupplyOrder(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{supplyOrderGuid:guid}/cancel", CancelSupplyOrder)
            .WithName("CancelSupplyOrder")
            .WithSummary("Cancels a supply order")
            .WithDescription("Cancels the supply order. Not allowed once the order is received, closed, or already cancelled.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> CancelSupplyOrder(
        Guid supplyOrderGuid,
        ICommandHandler<SupplyOrderCancelCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new SupplyOrderCancelCommand(supplyOrderGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static IEndpointRouteBuilder MapCloseSupplyOrder(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{supplyOrderGuid:guid}/close", CloseSupplyOrder)
            .WithName("CloseSupplyOrder")
            .WithSummary("Closes a supply order")
            .WithDescription("Finalizes the supply order. Allowed once at least some goods have been received.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> CloseSupplyOrder(
        Guid supplyOrderGuid,
        ICommandHandler<SupplyOrderCloseCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new SupplyOrderCloseCommand(supplyOrderGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion
}
