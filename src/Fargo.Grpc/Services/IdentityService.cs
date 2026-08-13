using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Identity;
using Fargo.Grpc.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fargo.Grpc.Services;

public sealed class IdentityService(
    ICommandHandler<IdentityLoginCommand, AuthResult> loginCommandHandler
) : V1.IdentityService.IdentityServiceBase
{
    public override async Task<AuthenticationResult> Login(LoginRequest request, ServerCallContext context)
    {
        var command = new IdentityLoginCommand(request.Nameid, request.Password);

        var result = await loginCommandHandler.HandleAsync(command, context.CancellationToken);

        return await Task.FromResult(new AuthenticationResult
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt.ToTimestamp()
        });
    }

    /*
    rpc Logout(LogoutRequest)

        returns (google.protobuf.Empty);

    rpc RefreshToken(RefreshTokenRequest)

        returns (AuthenticationResult);

    rpc ChangePassword(ChangePasswordRequest)

        returns (google.protobuf.Empty);

    */

}
