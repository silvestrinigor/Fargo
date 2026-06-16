using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Shared;

namespace Fargo.Application.Articles.Functions;

/// <summary>
/// Provides the default workflow for creating an article and
/// recording its corresponding creation event.
/// </summary>
internal static class ArticleCreateFunction
{
    /// <summary>
    /// Creates a new article, adds it to the article repository,
    /// and records an entity-created event associated with the actor.
    /// </summary>
    /// <param name="name">
    /// The name of the article to create.
    /// </param>
    /// <param name="actor">
    /// The actor responsible for creating the article.
    /// </param>
    /// <param name="articleRepository">
    /// Repository used to persist the created article.
    /// </param>
    /// <param name="eventRepository">
    /// Repository used to persist the generated creation event.
    /// </param>
    /// <returns>
    /// The newly created article.
    /// </returns>
    internal static Article CreateArticle(Name name, Actor actor, IArticleRepository articleRepository, IEventRepository eventRepository)
    {
        var article = Article.NewArticle(name);

        articleRepository.Add(article);

        var created = Event.NewEntityCreated(article, actor.ActorId, DateTimeOffset.UtcNow);

        eventRepository.Add(created);

        return article;
    }
}
