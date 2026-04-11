using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

/// <summary>
/// SignalR hub that broadcasts domain events to all connected authenticated clients.
/// </summary>
[Authorize]
public sealed class FargoEventHub : Hub<IFargoEventClient> { }
