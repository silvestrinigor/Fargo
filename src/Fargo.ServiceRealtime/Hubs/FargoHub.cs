using Fargo.Core.Entities.Actors;
using Fargo.Infrastructure.Security;
using Fargo.RealtimeContracts;
using Fargo.ServiceRealtime.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.ServiceRealtime.Hubs;

/// <summary>
/// The real-time hub. Clients only listen: the server pushes
/// <see cref="IFargoRealtimeClient.EntityChanged"/> and the hub exposes no
/// client-callable methods.
/// </summary>
/// <remarks>
/// <para>
/// A connection requires a valid Fargo access token, validated as the API validates it (signature,
/// lifetime, the actor's active state and auth version). Browsers pass it in the <c>access_token</c>
/// query parameter, accepted only under <c>/hubs</c>.
/// </para>
/// <para>
/// On connect the actor's partition access is resolved and kept with the connection; an actor that is
/// not found or is inactive is disconnected. <see cref="Services.RealtimeAccessRefreshService"/> refreshes
/// the access periodically and disconnects actors that became inactive. The connection closes when the
/// token expires.
/// </para>
/// </remarks>
[Authorize]
public sealed class FargoHub(
    ActorResolver actorResolver,
    RealtimeConnectionRegistry registry,
    ILogger<FargoHub> logger
) : Hub<IFargoRealtimeClient>
{
    public const string Path = FargoRealtimeHub.Path;

    public override async Task OnConnectedAsync()
    {
        var actorGuid = Context.User.GetFargoActorGuid();
        var actorType = Context.User.GetFargoActorType();

        var principal = actorGuid == Guid.Empty
            ? null
            : await actorResolver.GetActorByGuidAndTypeAsync(actorGuid, actorType, Context.ConnectionAborted);

        if (principal is null)
        {
            logger.ConnectionRejected(Context.ConnectionId, actorGuid);

            Context.Abort();

            return;
        }

        registry.Add(new RealtimeConnection(
            Context.ConnectionId,
            principal.Guid,
            principal.ActorType,
            principal.PartitionAccessGuids,
            Context.Abort));

        logger.ConnectionOpened(Context.ConnectionId, principal.Guid, principal.ActorType);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        registry.Remove(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}
