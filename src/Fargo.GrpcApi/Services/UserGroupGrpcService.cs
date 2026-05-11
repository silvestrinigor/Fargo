using Fargo.Application;
using Fargo.Application.UserGroups;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

[Authorize]
public sealed class UserGroupGrpcService(
    IQueryHandler<UserGroupSingleQuery, UserGroupDto?> singleHandler,
    IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>> manyHandler,
    ICommandHandler<UserGroupCreateCommand, Guid> createHandler,
    ICommandHandler<UserGroupUpdateCommand> updateHandler,
    ICommandHandler<UserGroupDeleteCommand> deleteHandler)
    : GrpcContracts.UserGroupsGrpc.UserGroupsGrpcBase
{
    public override async Task<GrpcContracts.UserGroupInfo> GetUserGroup(
        GrpcContracts.GetUserGroupRequest request,
        ServerCallContext context)
    {
        var userGroupGuid = request.UserGroupGuid.ToGuid(nameof(request.UserGroupGuid));
        var result = await singleHandler.Handle(
            new UserGroupSingleQuery(userGroupGuid, request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        return result?.ToInfo() ?? throw new UserGroupNotFoundFargoApplicationException(userGroupGuid);
    }

    public override async Task<GrpcContracts.UserGroupList> GetUserGroups(
        GrpcContracts.GetManyRequest request,
        ServerCallContext context)
    {
        var result = await manyHandler.Handle(
            new UserGroupsQuery(
                request.ToPagination(),
                request.TemporalAsOf.ToDateTimeOffset(),
                request.ChildOfAnyOfThesePartitions.ToGuidCollectionOrNull(),
                request.HasNotChildOfAnyPartition ? request.NotChildOfAnyPartition : null),
            context.CancellationToken);

        var response = new GrpcContracts.UserGroupList();
        response.UserGroups.AddRange(result.Select(static userGroup => userGroup.ToInfo()));
        return response;
    }

    public override async Task<GrpcContracts.GuidResult> CreateUserGroup(
        GrpcContracts.UserGroupCreateRequest request,
        ServerCallContext context)
    {
        var guid = await createHandler.Handle(
            new UserGroupCreateCommand(request.ToApplicationDto()),
            context.CancellationToken);

        return guid.ToGuidResult();
    }

    public override async Task<Empty> UpdateUserGroup(
        GrpcContracts.UserGroupUpdateRequest request,
        ServerCallContext context)
    {
        await updateHandler.Handle(
            new UserGroupUpdateCommand(
                request.UserGroupGuid.ToGuid(nameof(request.UserGroupGuid)),
                request.ToApplicationDto()),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteUserGroup(
        GrpcContracts.GuidRequest request,
        ServerCallContext context)
    {
        await deleteHandler.Handle(
            new UserGroupDeleteCommand(request.Guid.ToGuid(nameof(request.Guid))),
            context.CancellationToken);

        return new Empty();
    }
}
