using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleDeleteCommand(
            Guid ArticleGuid
            ) : ICommand;

    public sealed class ArticleDeleteCommandHandler(
            ArticleService articleService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleDeleteCommand>
    {
        public async Task Handle(
                ArticleDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = currentUser.ToActor();

            var article = await articleService.GetArticle(
                    actor,
                    command.ArticleGuid,
                    cancellationToken
                    )
                ?? throw new ArticleNotFoundFargoApplicationException(
                        command.ArticleGuid
                        );

            await articleService.DeleteArticle(
                    actor,
                    article,
                    cancellationToken
                    );

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}