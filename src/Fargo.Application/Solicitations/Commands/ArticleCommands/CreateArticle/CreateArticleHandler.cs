using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Solicitations.Commands;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Commands.ArticleCommands.CreateArticle
{
    public sealed class CreateArticleHandler(IArticleRepository articleRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateArticleCommand, Task>
    {
        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(CreateArticleCommand command)
        {
            var article = new Article { Name = command.Name };

            articleRepository.Add(article);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
