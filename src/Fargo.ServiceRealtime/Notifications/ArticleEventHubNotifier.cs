using Fargo.Application.IntegrationEvents;
using Fargo.RealtimeContracts;
using Fargo.ServiceRealtime.Connections;
using Fargo.ServiceRealtime.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.ServiceRealtime.Notifications;

/// <summary>
/// Pushes each article event received from the broker to the hub connections
/// whose actor can access at least one of the article's partitions.
/// </summary>
/// <remarks>
/// The partitions come from the event itself
/// (<see cref="ArticleEventMessage.PartitionGuids"/>), so routing needs no
/// database read. An event with no partitions reaches nobody.
/// </remarks>
/// <param name="hubContext">Sends to hub connections.</param>
/// <param name="registry">The open connections and their partition access.</param>
/// <param name="logger">The logger.</param>
public sealed class ArticleEventHubNotifier(
    IHubContext<FargoHub, IFargoRealtimeClient> hubContext,
    RealtimeConnectionRegistry registry,
    ILogger<ArticleEventHubNotifier> logger
) : IArticleEventHandler
{
    /// <inheritdoc />
    public async Task HandleAsync(ArticleEventMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (ArticleEventNotifications.ToNotification(message) is not { } notification)
        {
            logger.UnknownEventType(message.EventId, message.EventType);

            return;
        }

        var connectionIds = registry.GetConnectionIdsThatCanSee(message.PartitionGuids);

        if (connectionIds.Count == 0)
        {
            return;
        }

        await hubContext.Clients.Clients(connectionIds).EntityChanged(notification);

        logger.NotificationSent(message.EventId, message.EventType, connectionIds.Count);
    }
}
