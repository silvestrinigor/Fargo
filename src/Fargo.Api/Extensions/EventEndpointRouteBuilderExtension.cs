using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Events;
using Fargo.Domain;
using Fargo.Domain.Events;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

public static class EventEndpointRouteBuilderExtension
{
    public static void MapFargoEvent(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/api/events")
            .RequireAuthorization()
            .WithTags("Events");

        group.MapGet("/", GetManyEvents)
            .WithName("GetEvents")
            .WithSummary("Gets a filtered, paged list of domain events")
            .Produces<IReadOnlyCollection<EventInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<EventInformation>>, NoContent>> GetManyEvents(
        IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>> handler,
        Guid? entityGuid,
        EntityType? entityType,
        EventType? eventType,
        Guid? actorGuid,
        Guid? apiClientGuid,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Page? page,
        Limit? limit,
        CancellationToken cancellationToken)
    {
        var query = new EventManyQuery(
            EntityGuid: entityGuid,
            EntityType: entityType,
            EventType: eventType,
            ActorGuid: actorGuid,
            ApiClientGuid: apiClientGuid,
            From: from,
            To: to,
            Pagination: PaginationHelpers.CreatePagination(page, limit));

        var result = await handler.Handle(query, cancellationToken);
        return TypedResultsHelpers.HandleCollectionQueryResult(result);
    }
}
