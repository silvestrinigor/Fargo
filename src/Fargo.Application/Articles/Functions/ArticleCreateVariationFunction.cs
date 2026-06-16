using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Shared;

namespace Fargo.Application.Articles.Functions;

internal static class ArticleCreateVariationFunction
{
    internal static Article CreateArticleVariation(Name name, Article fromArticle, Actor actor, IArticleRepository articleRepository, IEventRepository eventRepository)
    {
        var article = Article.NewArticleVariation(name, fromArticle);

        articleRepository.Add(article);

        var created = Event.NewEntityCreated(article, actor.ActorId, DateTimeOffset.UtcNow);

        eventRepository.Add(created);

        return article;
    }
}
