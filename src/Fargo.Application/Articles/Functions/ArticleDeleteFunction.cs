using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Shared;

namespace Fargo.Application.Articles.Functions;

internal static class ArticleDeleteFunction
{

    internal static async Task DeleteArticle(Article article, Actor actor, IArticleRepository articleRepository, IEventRepository eventRepository)
    {

        articleRepository.Add(article);

        var created = Event.NewEntityCreated(article, actor.ActorId, DateTimeOffset.UtcNow);

        eventRepository.Add(created);
    }
}
