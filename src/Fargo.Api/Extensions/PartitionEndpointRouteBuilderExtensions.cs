using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>Maps all Partition endpoints.</summary>
public static class PartitionEndpointRouteBuilderExtension
{
    public static void MapFargoPartition(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/partitions")
            .RequireAuthorization()
            .WithTags("Partitions");

        group.MapGet("/{partitionGuid:guid}", GetSinglePartition)
            .WithName("GetPartition")
            .Produces<PartitionInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyPartition)
            .WithName("GetPartitions")
            .Produces<IReadOnlyCollection<PartitionInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreatePartition)
            .WithName("CreatePartition")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{partitionGuid:guid}", UpdatePartition)
            .WithName("UpdatePartition")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{partitionGuid:guid}", DeletePartition)
            .WithName("DeletePartition")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<PartitionInformation>, NotFound>> GetSinglePartition(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<PartitionSingleQuery, PartitionInformation?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new PartitionSingleQuery(partitionGuid, temporalAsOf), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NoContent>> GetManyPartition(
        Guid? parentPartitionGuid,
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        bool? rootOnly,
        string? search,
        IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new PartitionManyQuery(
            parentPartitionGuid,
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            rootOnly ?? false,
            search
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<Guid>> CreatePartition(
        PartitionCreateCommand request,
        ICommandHandler<PartitionCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdatePartition(
        Guid partitionGuid,
        PartitionUpdateModel request,
        ICommandHandler<PartitionUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PartitionUpdateCommand(partitionGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeletePartition(
        Guid partitionGuid,
        ICommandHandler<PartitionDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PartitionDeleteCommand(partitionGuid), cancellationToken);

        return TypedResults.NoContent();
    }
}
