using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Items.Events;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class ItemEventEndpointRouteBuilderExtension
{
    public static void MapFargoItemEvent(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapItemEventGroup();

        group.MapGetItemEvents();

        group.MapCreateItemMovementEvent();
    }

    private static RouteGroupBuilder MapItemEventGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapGroup("/item-events")
        .RequireAuthorization()
        .WithTags("ItemEvents")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get Many

    private static IEndpointRouteBuilder MapGetItemEvents(this IEndpointRouteBuilder builder)
    {
        builder
        .MapGet("/", GetManyItemEventsAsync)
        .WithName("GetItemEvents")
        .WithSummary("Gets multiple item events")
        .WithDescription("Retrieves a paginated list of item events, optionally filtered by article, event type, and time period, including the total number of matching entries. Requires the ReadItemEvents permission; only events whose article is accessible to the caller are returned.")
        .Produces<PagedResult<ItemEventDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<ItemEventDto>>> GetManyItemEventsAsync(
        Page? page,
        Limit? limit,
        Guid? articleGuid,
        ItemEventType? eventType,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd,
        IQueryHandler<ItemEventsQuery, PagedResult<ItemEventDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new ItemEventsQuery(withPagination, articleGuid, eventType, periodStart, periodEnd);

        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create movement event

    private static IEndpointRouteBuilder MapCreateItemMovementEvent(this IEndpointRouteBuilder builder)
    {
        builder
        .MapPost("/", CreateItemMovementEvent)
        .WithName("CreateItemEvent")
        .WithSummary("Records an item event")
        .WithDescription("Records a new item event. Currently the only supported event type is a movement of a quantity of an event-tracked article between containers (or into/out of no container). Only valid for articles whose tracking mode is Event; entity-tracked articles record movements through their individual items instead.")
        .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> CreateItemMovementEvent(
        ItemEventCreateDto request,
        ICommandHandler<MoveItemEventCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new MoveItemEventCommand(request.ToMovement());

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Create movement event
}
