using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Solicitations.Commands;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Commands.ItemCommands.CreateItem
{
    public sealed class CreateItemHandler(IArticleRepository articleRepository, IItemRepository itemRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateItemCommand, Task>
    {

        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IItemRepository itemRepository = itemRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(CreateItemCommand command)
        {
            var article = await articleRepository.GetByGuidAsync(command.ArticleGuid)
                ?? throw new InvalidOperationException($"Article with Guid {command.ArticleGuid} not found.");

            var item = new Item { Name = command.Name, Article = article };

            itemRepository.Add(item);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
