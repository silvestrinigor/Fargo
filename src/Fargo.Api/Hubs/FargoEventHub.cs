using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

public class FargoEventHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
