using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

/// <summary>
/// SignalR hub that broadcasts domain events to clients based on their partition access.
/// Each connection joins partition-specific groups on connect so events are only
/// delivered to clients who have access to the affected entity's partition.
/// </summary>
[Authorize]
public sealed class FargoEventHub(ActorService actorService, ICurrentUser currentUser)
    : Hub<IFargoEventClient>
{
    /// <summary>
    /// Adds the connecting client to SignalR groups for each partition they can access.
    /// Admin and system actors join a single <c>"fargo-admin"</c> group that receives all events.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid);

        if (actor.IsAdmin || actor.IsSystem)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "fargo-admin");
        }
        else
        {
            foreach (var partitionGuid in actor.PartitionAccesses)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, partitionGuid.ToString());
            }
        }

        await base.OnConnectedAsync();
    }
}
