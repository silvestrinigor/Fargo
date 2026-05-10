using Fargo.Application;
using Fargo.Application.Items;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

[Authorize]
public sealed class ItemGrpcService(
    IQueryHandler<ItemSingleQuery, ItemDto?> singleHandler,
    IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>> manyHandler,
    ICommandHandler<ItemCreateCommand, Guid> createHandler,
    ICommandHandler<ItemUpdateCommand> updateHandler,
    ICommandHandler<ItemDeleteCommand> deleteHandler)
    : GrpcContracts.ItemsGrpc.ItemsGrpcBase
{
    public override async Task<GrpcContracts.ItemInfo> GetItem(
        GrpcContracts.GetItemRequest request,
        ServerCallContext context)
    {
        var itemGuid = request.ItemGuid.ToGuid(nameof(request.ItemGuid));
        var result = await singleHandler.Handle(
            new ItemSingleQuery(itemGuid, request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        return result?.ToInfo() ?? throw new ItemNotFoundFargoApplicationException(itemGuid);
    }

    public override async Task<GrpcContracts.ItemList> GetItems(
        GrpcContracts.GetManyRequest request,
        ServerCallContext context)
    {
        var result = await manyHandler.Handle(
            new ItemsQuery(
                request.ToPagination(),
                request.TemporalAsOf.ToDateTimeOffset(),
                request.InsideAnyOfThisPartitions.ToGuidCollectionOrNull(),
                request.HasNotInsideAnyPartition ? request.NotInsideAnyPartition : null),
            context.CancellationToken);

        var response = new GrpcContracts.ItemList();
        response.Items.AddRange(result.Select(static item => item.ToInfo()));
        return response;
    }

    public override async Task<GrpcContracts.GuidResult> CreateItem(
        GrpcContracts.ItemCreateRequest request,
        ServerCallContext context)
    {
        var guid = await createHandler.Handle(
            new ItemCreateCommand(request.ToApplicationDto()),
            context.CancellationToken);

        return guid.ToGuidResult();
    }

    public override async Task<Empty> UpdateItem(
        GrpcContracts.ItemUpdateRequest request,
        ServerCallContext context)
    {
        await updateHandler.Handle(
            new ItemUpdateCommand(
                request.ItemGuid.ToGuid(nameof(request.ItemGuid)),
                request.ToApplicationDto()),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteItem(
        GrpcContracts.GuidRequest request,
        ServerCallContext context)
    {
        await deleteHandler.Handle(
            new ItemDeleteCommand(request.Guid.ToGuid(nameof(request.Guid))),
            context.CancellationToken);

        return new Empty();
    }
}
