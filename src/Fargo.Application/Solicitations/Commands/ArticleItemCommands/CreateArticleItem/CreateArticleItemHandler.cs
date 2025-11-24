using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Solicitations.Commands;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Commands.ArticleItemCommands.CreateArticleItem
{
    public sealed class CreateArticleItemHandler(IArticleRepository articleRepository, IArticleItemRepository articleItemRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateArticleItemCommand, Task>
    {
        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IArticleItemRepository articleItemRepository = articleItemRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(CreateArticleItemCommand command)
        {
            // article must exist

            // load article

            var article = await articleRepository.GetAsync(command.Article) ?? throw new InvalidOperationException("Article not found.");
            
            var articleItem = new ArticleItem { Name = command.Name, Article = article };

            articleItemRepository.Add(articleItem);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
