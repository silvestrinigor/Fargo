namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// Reacts to an article event received from the broker.
/// </summary>
/// <remarks>
/// Implemented by the host that consumes events (for example the real-time
/// service, which pushes the change to connected clients). Delivery is
/// at-least-once, so an implementation must tolerate the same
/// <see cref="ArticleEventMessage.EventId"/> twice.
/// </remarks>
public interface IArticleEventHandler
{
    /// <summary>
    /// Handles one article event.
    /// </summary>
    /// <param name="message">The received event.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task HandleAsync(ArticleEventMessage message, CancellationToken cancellationToken = default);
}
