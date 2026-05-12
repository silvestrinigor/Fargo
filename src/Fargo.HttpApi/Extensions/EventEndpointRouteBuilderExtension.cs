using Fargo.Application;
using Fargo.Application.Events;
using Fargo.HttpApi.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using ContractEntityType = Fargo.Sdk.Contracts.EntityType;
using ContractEvents = Fargo.Sdk.Contracts.Events;

namespace Fargo.HttpApi.Extensions;

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
            .Produces<IReadOnlyCollection<ContractEvents.EventInfo>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ContractEvents.EventInfo>>, NoContent>> GetManyEvents(
        IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>> handler,
        Guid? entityGuid,
        ContractEntityType? entityType,
        ContractEvents.EventType? eventType,
        Guid? actorGuid,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Page? page,
        Limit? limit,
        CancellationToken cancellationToken)
    {
        var query = new EventManyQuery(
            EntityGuid: entityGuid,
            EntityType: entityType?.ToDomain(),
            EventType: eventType?.ToDomain(),
            ActorGuid: actorGuid,
            From: from,
            To: to,
            Pagination: new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit));

        var result = await handler.Handle(query, cancellationToken);
        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(result.ToInfo());
    }
}
