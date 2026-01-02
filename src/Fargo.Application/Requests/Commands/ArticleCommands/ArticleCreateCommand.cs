using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
        ArticleCreateDto Article
        ) : ICommand<Guid>;

    public sealed class ArticleCreateCommandHandler(IArticleRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<ArticleCreateCommand, Guid>
    {
        public async Task<Guid> HandleAsync(ArticleCreateCommand command, CancellationToken cancellationToken = default)
        {
            var article = new Article
            {
                Name = command.Article.Name,
                Description = command.Article.Description,

                ShelfLife = command.Article.ShelfLife!,
                MaximumContainerTemperature = command.Article.MaxContainerTemperature,
                MinimumContainerTemperature = command.Article.MinContainerTemperature,

                Measures = new ArticleMeasures
                {
                    LengthX = command.Article.Measures?.X,
                    LengthY = command.Article.Measures?.Y,
                    LengthZ = command.Article.Measures?.Z,
                    Mass = command.Article.Measures?.Mass,
                    Volume = command.Article.Measures?.Volume,
                    Density = command.Article.Measures?.Density,
                },

                Container = command.Article.Container is not null
                ? new ArticleContainer
                {
                    MassCapacity = command.Article.Container.MassCapacity,
                    VolumeCapacity = command.Article.Container.VolumeCapacity,
                    ItensQuantityCapacity = command.Article.Container.ItensQuantityCapacity,
                    DefaultTemperature = command.Article.Container.DefaultTemperature
                }
                : null
            };
            
            repository.Add(article);

            await unitOfWork.SaveChangesAsync();

            return article.Guid;
        }
    }
}