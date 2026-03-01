using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.ArticleServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.ArticleCommands
{
    public sealed record ArticleDeleteCommand(
            Guid ArticleGuid
            ) : ICommand;

    public sealed class ArticleDeleteCommandHandler(
            ArticleDeteleService articleDeteleService,
            ArticleGetService articleGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ArticleDeleteCommand>
    {
        public async Task Handle(
                ArticleDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleGetService.GetArticle(
                    actor,
                    command.ArticleGuid,
                    cancellationToken
                    ) ?? throw new ArticleNotFoundFargoApplicationException(command.ArticleGuid);

            await articleDeteleService.DeleteArticle(actor, article, cancellationToken);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}