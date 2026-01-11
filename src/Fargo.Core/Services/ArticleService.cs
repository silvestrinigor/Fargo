using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository articleRepository)
    {
        public async Task<Article?> GetArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default)
            => await articleRepository.GetByGuidAsync(articleGuid, cancellationToken);

        public Article CreateArticle(Name name, Description description, bool isContainer = false)
        {
            var article = new Article
            {
                Name = name,
                Description = description,
                IsContainer = isContainer
            };

            articleRepository.Add(article);

            return article;
        }

        public async Task DeleteArticleAsync(Article article, CancellationToken cancellationToken = default)
        {
            var hasItens = await articleRepository.HasItensAssociated(article.Guid, cancellationToken);

            if (hasItens)
            {
                throw new InvalidOperationException("Cannot delete article with associated items.");
            }

            articleRepository.Remove(article);
        }
    }
}
