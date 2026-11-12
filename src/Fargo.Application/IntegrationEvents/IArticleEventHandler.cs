namespace Fargo.Application.IntegrationEvents;

/// <remarks>
/// Implemented by the host that consumes events (for example the real-time service, which pushes the
/// change to connected clients). Delivery is at-least-once, so an implementation must tolerate the
/// same <see cref="ArticleEventMessage.EventId"/> twice.
/// </remarks>
public interface IArticleEventHandler
{
    Task HandleAsync(ArticleEventMessage message, CancellationToken cancellationToken = default);
}
