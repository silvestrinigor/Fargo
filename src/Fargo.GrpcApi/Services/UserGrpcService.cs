using Fargo.Application;
using Fargo.Application.Users;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

[Authorize]
public sealed class UserGrpcService(
    IQueryHandler<UserSingleQuery, UserDto?> singleHandler,
    IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>> manyHandler,
    ICommandHandler<UserCreateCommand, Guid> createHandler,
    ICommandHandler<UserUpdateCommand> updateHandler,
    ICommandHandler<UserDeleteCommand> deleteHandler)
    : GrpcContracts.UsersGrpc.UsersGrpcBase
{
    public override async Task<GrpcContracts.UserInfo> GetUser(
        GrpcContracts.GetUserRequest request,
        ServerCallContext context)
    {
        var userGuid = request.UserGuid.ToGuid(nameof(request.UserGuid));
        var result = await singleHandler.Handle(
            new UserSingleQuery(userGuid, request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        return result?.ToInfo() ?? throw new UserNotFoundFargoApplicationException(userGuid);
    }

    public override async Task<GrpcContracts.UserList> GetUsers(
        GrpcContracts.GetManyRequest request,
        ServerCallContext context)
    {
        var result = await manyHandler.Handle(
            new UsersQuery(
                request.ToPagination(),
                request.TemporalAsOf.ToDateTimeOffset(),
                request.ChildOfAnyOfThesePartitions.ToGuidCollectionOrNull(),
                request.HasNotChildOfAnyPartition ? request.NotChildOfAnyPartition : null),
            context.CancellationToken);

        var response = new GrpcContracts.UserList();
        response.Users.AddRange(result.Select(static user => user.ToInfo()));
        return response;
    }

    public override async Task<GrpcContracts.GuidResult> CreateUser(
        GrpcContracts.UserCreateRequest request,
        ServerCallContext context)
    {
        var guid = await createHandler.Handle(
            new UserCreateCommand(request.ToApplicationDto()),
            context.CancellationToken);

        return guid.ToGuidResult();
    }

    public override async Task<Empty> UpdateUser(
        GrpcContracts.UserUpdateRequest request,
        ServerCallContext context)
    {
        await updateHandler.Handle(
            new UserUpdateCommand(
                request.UserGuid.ToGuid(nameof(request.UserGuid)),
                request.ToApplicationDto()),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteUser(
        GrpcContracts.GuidRequest request,
        ServerCallContext context)
    {
        await deleteHandler.Handle(
            new UserDeleteCommand(request.Guid.ToGuid(nameof(request.Guid))),
            context.CancellationToken);

        return new Empty();
    }
}
