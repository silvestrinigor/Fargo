using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Shared;

namespace Fargo.Application.Articles.Functions;

internal static class ArticleCreateDefaultFunction
{
    internal static Article CreateArticle(Name name, Actor actor, IArticleRepository articleRepository, IEventRepository eventRepository)
    {
        var article = Article.NewArticle(name, actor);

        articleRepository.Add(article);

        var articleCreated = Event.NewEntityCreatedEvent(article, actor.Guid);

        eventRepository.Add(articleCreated);

        return article;
    }
}
