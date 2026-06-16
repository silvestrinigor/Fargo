using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Shared;
using UnitsNet;

namespace Fargo.Application.Articles.Functions;

internal static class ArticleCreatePackFunction
{
    internal static Article CreateArticlePack(Name name, Article fromArticle, Scalar quantity, Actor actor, IArticleRepository articleRepository, IEventRepository eventRepository)
    {
        var article = Article.NewArticlePack(name, fromArticle, quantity);

        articleRepository.Add(article);

        var created = Event.NewEntityCreated(article, actor.ActorId, DateTimeOffset.UtcNow);

        eventRepository.Add(created);

        return article;
    }
}
