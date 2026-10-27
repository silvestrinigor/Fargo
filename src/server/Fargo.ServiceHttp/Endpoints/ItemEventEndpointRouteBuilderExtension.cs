using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Items.Events;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class ItemEventEndpointRouteBuilderExtension
{
    public static void MapFargoItemEvent(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapItemEventGroup();

        group.MapGetItemEvents();

        group.MapCreateItemEvent();
    }

    private static RouteGroupBuilder MapItemEventGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("ItemEvents", "/item/events")
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

    #region Create event

    private static IEndpointRouteBuilder MapCreateItemEvent(this IEndpointRouteBuilder builder)
    {
        builder
        .MapPost("/", CreateItemEvent)
        .WithName("CreateItemEvent")
        .WithSummary("Records an item event")
        .WithDescription("Records a new item event for an event-tracked article. The eventType selects the kind: a movement of a quantity between containers, or the creation or deletion of a quantity of stock (optionally at a container). Event-based stock is not unit-tracked, so a single creation or deletion can cover many units. Returns the identifier of the recorded event. Only valid for articles whose tracking mode is Event.")
        .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateItemEvent(
        ItemEventCreateDto request,
        ICommandHandler<ItemEventCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new ItemEventCreateCommand(request);

        var response = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create event
}
