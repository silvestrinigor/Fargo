using Fargo.Application;
using Fargo.Application.Authentication;
using Fargo.Application.Users;
using Fargo.Domain.Tokens;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

public sealed class AuthenticationGrpcService(
    ICommandHandler<LoginCommand, AuthResult> loginHandler,
    ICommandHandler<LogoutCommand> logoutHandler,
    ICommandHandler<RefreshCommand, AuthResult> refreshHandler,
    ICommandHandler<PasswordChangeCommand> passwordChangeHandler)
    : GrpcContracts.AuthenticationGrpc.AuthenticationGrpcBase
{
    public override async Task<GrpcContracts.AuthInfo> Login(
        GrpcContracts.LoginRequest request,
        ServerCallContext context)
    {
        var result = await loginHandler.Handle(
            new LoginCommand(request.Nameid, request.Password),
            context.CancellationToken);

        return result.ToInfo();
    }

    public override async Task<Empty> Logout(
        GrpcContracts.RefreshRequest request,
        ServerCallContext context)
    {
        await logoutHandler.Handle(
            new LogoutCommand(new Token(request.RefreshToken)),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<GrpcContracts.AuthInfo> Refresh(
        GrpcContracts.RefreshRequest request,
        ServerCallContext context)
    {
        var result = await refreshHandler.Handle(
            new RefreshCommand(new Token(request.RefreshToken)),
            context.CancellationToken);

        return result.ToInfo();
    }

    [Authorize]
    public override async Task<Empty> ChangePassword(
        GrpcContracts.PasswordUpdateRequest request,
        ServerCallContext context)
    {
        await passwordChangeHandler.Handle(
            new PasswordChangeCommand(new UserPasswordUpdateDto(
                request.NewPassword,
                request.HasCurrentPassword ? request.CurrentPassword : null)),
            context.CancellationToken);

        return new Empty();
    }
}
