using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository repository)
    {
        public async Task<Article?> GetArticle(
                Actor actor,
                Guid articleGuid,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    articleGuid,
                    actor.PartitionGuids,
                    cancellationToken
                    );

        public async Task<Actor?> GetActor(
                int UserId,
                Password UserPassword
                )
        {

        }

        public Article CreateArticle(
                Actor actor,
                Name name,
                Description description = default,
                bool isContainer = false
                )
        {
            if (!actor.HasPermission(ActionType.CreateArticle))
            {
                throw new ActorNotAuthorizedException(
                        actor,
                        ActionType.CreateArticle
                        );
            }

            var article = new Article
            {
                Name = name,
                Description = description,
                IsContainer = isContainer
            };

            repository.Add(article);

            return article;
        }

        public async Task DeleteArticle(
                Actor actor,
                Article article,
                CancellationToken cancellationToken = default
                )
        {

            if (!actor.HasPermission(ActionType.DeleteArticle))
            {
                throw new ActorNotAuthorizedException(
                        actor,
                        ActionType.DeleteArticle
                        );
            }

            var hasItens = await repository.HasItemsAssociated(
                    article.Guid,
                    cancellationToken
                    );

            if (hasItens)
            {
                throw new ArticleDeleteWithItemsAssociatedException(article);
            }

            repository.Remove(article);
        }
    }
}