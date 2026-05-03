namespace Fargo.Api.Articles;

/// <summary>
/// Combined article interface: CRUD + Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IArticleManager : IArticleService, IArticleEventSource { }
