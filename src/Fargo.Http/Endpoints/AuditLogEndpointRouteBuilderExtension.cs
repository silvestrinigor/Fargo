using Fargo.Application.Audits;
using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Http.Endpoints;

public static class AuditLogEndpointRouteBuilderExtension
{
    public static void MapFargoAuditLog(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapAuditLogGroup();

        group.MapGetAuditLogs();
    }

    private static RouteGroupBuilder MapAuditLogGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/audit-logs")
            .RequireAuthorization()
            .WithTags("AuditLogs");

        return group;
    }

    #region Get Many

    private static IEndpointRouteBuilder MapGetAuditLogs(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyAuditLogAsync)
            .WithName("GetAuditLogs")
            .WithSummary("Gets multiple audit logs")
            .WithDescription("Retrieves a paginated list of audit logs.")
            .Produces<IReadOnlyCollection<AuditLogDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<AuditLogDto>>, NoContent>> GetManyAuditLogAsync(
        Page? page, Limit? limit,
        Guid? actorGuid,
        ActorType? actorType,
        Guid? entityGuid,
        EntityType? entityType,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd,
        IQueryHandler<AuditLogsQuery, IReadOnlyCollection<AuditLogDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new AuditLogsQuery(withPagination, actorGuid, actorType, entityGuid, entityType, periodStart, periodEnd);

        var response = await handler.HandleAsync(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    #endregion
}
