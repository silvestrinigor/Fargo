using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>Maps all Item endpoints.</summary>
public static class ItemEndpointRouteBuilderExtension
{
    public static void MapFargoItem(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/items")
            .RequireAuthorization()
            .WithTags("Items");

        group.MapGet("/{itemGuid:guid}", GetSingleItem)
            .WithName("GetItem")
            .Produces<ItemInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyItems)
            .WithName("GetItems")
            .Produces<IReadOnlyCollection<ItemInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateItem)
            .WithName("CreateItem")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{itemGuid:guid}", UpdateItem)
            .WithName("UpdateItem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{itemGuid:guid}", DeleteItem)
            .WithName("DeleteItem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{itemGuid:guid}/partitions", GetItemPartitions)
            .WithName("GetItemPartitions")
            .Produces<IReadOnlyCollection<PartitionInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{itemGuid:guid}/partitions/{partitionGuid:guid}", AddItemPartition)
            .WithName("AddItemPartition")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{itemGuid:guid}/partitions/{partitionGuid:guid}", RemoveItemPartition)
            .WithName("RemoveItemPartition")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<ItemInformation>, NotFound>> GetSingleItem(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<ItemSingleQuery, ItemInformation?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new ItemSingleQuery(itemGuid, temporalAsOf), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ItemInformation>>, NoContent>> GetManyItems(
        Guid? articleGuid,
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        Guid? partitionGuid,
        bool? noPartition,
        IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new ItemManyQuery(
            articleGuid,
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            partitionGuid,
            noPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<Guid>> CreateItem(
        ItemCreateModel request,
        ICommandHandler<ItemCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new ItemCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateItem(
        Guid itemGuid,
        ItemUpdateModel request,
        ICommandHandler<ItemUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemUpdateCommand(itemGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteItem(
        Guid itemGuid,
        ICommandHandler<ItemDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemDeleteCommand(itemGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NotFound, NoContent>> GetItemPartitions(
        Guid itemGuid,
        IQueryHandler<ItemPartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ItemPartitionsQuery(itemGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> AddItemPartition(
        Guid itemGuid,
        Guid partitionGuid,
        ICommandHandler<ItemAddPartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemAddPartitionCommand(itemGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveItemPartition(
        Guid itemGuid,
        Guid partitionGuid,
        ICommandHandler<ItemRemovePartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ItemRemovePartitionCommand(itemGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }
}
