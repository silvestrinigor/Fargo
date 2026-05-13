namespace Fargo.Core.Articles;

public enum ArticleEventType
{
    None = 0,
    ArticleCreated = 1,
    ArticleDeleted = 2
}

public interface IArticleEventArgs;

public sealed class ArticleEvent<TArticleEventArgs> : Entity where TArticleEventArgs : IArticleEventArgs
{
    public required Guid ArticleGuid { get; init; }

    public required Guid ActorGuid { get; init; }

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;

    public required TArticleEventArgs Data { get; init; }
}