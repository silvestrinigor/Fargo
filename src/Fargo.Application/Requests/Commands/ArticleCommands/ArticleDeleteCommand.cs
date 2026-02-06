using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand<Task>;

    public sealed class ArticleDeleteCommandHandler(
        ArticleService articleService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ArticleDeleteCommand, Task>
    {
        private readonly ArticleService articleService = articleService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task Handle(
                ArticleDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var article = await articleService.GetArticleAsync(
                    actor,
                    command.ArticleGuid,
                    cancellationToken
                    );

            await articleService.DeleteArticleAsync(
                    actor,
                    article,
                    cancellationToken
                    );

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}