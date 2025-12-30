using Fargo.Application.Persistence;
using Fargo.Application.Solicitations.Commands.ArticleCommands;
using Fargo.Application.Solicitations.Queries.ArticleQueries;
using Fargo.Application.Solicitations.Responses;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Services
{
    public class ArticleService(IArticleRepository articleRepository, IUnitOfWork unitOfWork) : IArticleService
    {
        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> CreateArticleAsync(ArticleCreateCommand command)
        {
            var articleContainerInformation 
                = command.ContainerInformation is not null
                ? new ArticleContainerInformation
                {
                    MassCapacity = command.ContainerInformation.MassCapacity,
                    VolumeCapacity = command.ContainerInformation.VolumeCapacity,
                    ItensQuantityCapacity = command.ContainerInformation.ItensQuantityCapacity,
                    DefaultTemperature = command.ContainerInformation.DefaultTemperature,
                }
                : null;

            var article = new Article
                (
                    name: command.Name,
                    description: command.Description
                )
                {
                    ShelfLife = command.ShelfLife,
                    LengthX = command.Length,
                    LengthY = command.Width,
                    LengthZ = command.Height,
                    Volume = command.Volume,
                    Mass = command.Mass,
                    Density = command.Density,
                    ContainerInformation = articleContainerInformation
                };

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

        public async Task<ArticleInformation?> GetArticleAsync(ArticleQuery query)
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
