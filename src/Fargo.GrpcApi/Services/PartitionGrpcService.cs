using Fargo.Application;
using Fargo.Application.Partitions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

[Authorize]
public sealed class PartitionGrpcService(
    IQueryHandler<PartitionSingleQuery, PartitionDto?> singleHandler,
    IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>> manyHandler,
    ICommandHandler<PartitionCreateCommand, Guid> createHandler,
    ICommandHandler<PartitionUpdateCommand> updateHandler,
    ICommandHandler<PartitionDeleteCommand> deleteHandler)
    : GrpcContracts.PartitionsGrpc.PartitionsGrpcBase
{
    public override async Task<GrpcContracts.PartitionInfo> GetPartition(
        GrpcContracts.GetPartitionRequest request,
        ServerCallContext context)
    {
        var partitionGuid = request.PartitionGuid.ToGuid(nameof(request.PartitionGuid));
        var result = await singleHandler.Handle(
            new PartitionSingleQuery(partitionGuid, request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        return result?.ToInfo() ?? throw new PartitionNotFoundFargoApplicationException(partitionGuid);
    }

    public override async Task<GrpcContracts.PartitionList> GetPartitions(
        GrpcContracts.GetManyRequest request,
        ServerCallContext context)
    {
        var result = await manyHandler.Handle(
            new PartitionsQuery(
                request.ToPagination(),
                request.TemporalAsOf.ToDateTimeOffset(),
                request.ChildOfAnyOfThesePartitions.ToGuidCollectionOrNull(),
                request.HasNotChildOfAnyPartition ? request.NotChildOfAnyPartition : null),
            context.CancellationToken);

        var response = new GrpcContracts.PartitionList();
        response.Partitions.AddRange(result.Select(static partition => partition.ToInfo()));
        return response;
    }

    public override async Task<GrpcContracts.GuidResult> CreatePartition(
        GrpcContracts.PartitionCreateRequest request,
        ServerCallContext context)
    {
        var guid = await createHandler.Handle(
            new PartitionCreateCommand(request.ToApplicationDto()),
            context.CancellationToken);

        return guid.ToGuidResult();
    }

    public override async Task<Empty> UpdatePartition(
        GrpcContracts.PartitionUpdateRequest request,
        ServerCallContext context)
    {
        await updateHandler.Handle(
            new PartitionUpdateCommand(
                request.PartitionGuid.ToGuid(nameof(request.PartitionGuid)),
                request.ToApplicationDto()),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeletePartition(
        GrpcContracts.GuidRequest request,
        ServerCallContext context)
    {
        await deleteHandler.Handle(
            new PartitionDeleteCommand(request.Guid.ToGuid(nameof(request.Guid))),
            context.CancellationToken);

        return new Empty();
    }
}
