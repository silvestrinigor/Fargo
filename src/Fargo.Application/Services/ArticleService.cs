using Fargo.Application.Persistence;
using Fargo.Application.Solicitations.Commands.ArticleCommands;
using Fargo.Application.Solicitations.Queries.ArticleQueries;
using Fargo.Application.Solicitations.Responses;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Services
{
    public class ArticleService(IArticleRepository articleRepository, IUnitOfWork unitOfWork) : IArticleService
    {
        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> CreateArticleAsync(ArticleCreateCommand command)
        {

            articleRepository.Add(article);

            await unitOfWork.SaveChangesAsync();

            return article.Guid;
        }

        public async Task DeleteArticleAsync(ArticleDeleteCommand command)
        {
            var article = await articleRepository.GetByGuidAsync(command.EntityGuid)
                ?? throw new InvalidOperationException("Article not found.");

            var hasItens = await articleRepository.HasItensAssociated(article.Guid);
            
            if (hasItens)
            {
                throw new InvalidOperationException("Cannot delete article with associated items.");
            }

            articleRepository.Remove(article);
            
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<ArticleInformation?> GetArticleAsync(ArticleSingleQuery query)
        {
            var article = await articleRepository.GetByGuidAsync(query.ArticleGuid);

            if (article is null)
            {
                return null;
            }

            return new ArticleInformation(
                article.Guid,
                article.Name,
                article.Description,
                article.CreatedAt,
                article.ParentGuid
            );
        }
    } 
}
