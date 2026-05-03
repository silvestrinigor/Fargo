using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Events;
using Fargo.Domain;
using Fargo.Domain.Events;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>Maps all Event endpoints.</summary>
public static class EventEndpointRouteBuilderExtension
{
    public static void MapFargoEvent(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/events")
            .RequireAuthorization()
            .WithTags("Events");

        group.MapGet("/", GetManyEvents)
            .WithName("GetEvents")
            .Produces<IReadOnlyCollection<EventInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<EventInformation>>, NoContent>> GetManyEvents(
        IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>> handler,
        Guid? entityGuid,
        EntityType? entityType,
        EventType? eventType,
        Guid? actorGuid,
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
            From: from,
            To: to,
            Pagination: PaginationHelpers.CreatePagination(page, limit));

        var result = await handler.Handle(query, cancellationToken);
        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(result);
    }
}
