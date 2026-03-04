using Fargo.Application.Exceptions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemCreateCommand(
            ItemCreateModel Item
            ) : ICommand<Guid>;

    public sealed class ItemCreateCommandHandler(
            IItemRepository itemRepository,
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                ItemCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    )
                ?? throw new UnauthorizedAccessFargoApplicationException();

            var article = await articleRepository.GetByGuid(
                    command.Item.ArticleGuid,
                    cancellationToken
                    )
                ?? throw new ArticleNotFoundFargoApplicationException(
                        command.Item.ArticleGuid);

            actor.ValidatePermission(ActionType.CreateItem);

            var item = new Item
            {
                Article = article
            };

            itemRepository.Add(item);

            await unitOfWork.SaveChanges(cancellationToken);

            return item.Guid;
        }
    }
}