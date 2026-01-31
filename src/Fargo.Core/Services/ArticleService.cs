using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository repository)
    {
        private readonly IArticleRepository repository = repository;

        public async Task<Article> GetArticleAsync(
                Guid articleGuid, 
                CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(articleGuid, cancellationToken)
            ?? throw new ArticleNotFoundException(articleGuid);

        public Article CreateArticle(
                Name name, 
                Description description = default, 
                bool isContainer = false)
        {
            var article = new Article
            {
                Name = name,
                Description = description,
                IsContainer = isContainer
            };

            repository.Add(article);

            return article;
        }

        public async Task DeleteArticleAsync(
                Article article, 
                CancellationToken cancellationToken = default)
        {
            var hasItens = await repository.HasItemsAssociated(article.Guid, cancellationToken);

            if (hasItens)
                throw new ArticleDeleteWithItemsAssociatedException(article);

            repository.Remove(article);
        }
    }
}