using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Services;
using Fargo.Application.Solicitations.Commands.ItemCommands;
using Fargo.Application.Solicitations.Queries.ItensQueries;
using Fargo.Application.Solicitations.Responses;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Services
{
    public class ItemService(IItemRepository itemRepository, IArticleRepository articleRepository, IUnitOfWork unitOfWork) : IItemService
    {
        private readonly IItemRepository itemRepository = itemRepository;
        private readonly IArticleRepository articleRepository = articleRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> CreateItemAsync(CreateItemCommand command)
        {
            var article = await articleRepository.GetByGuidAsync(command.ArticleGuid)
                ?? throw new InvalidOperationException("Article not found.");

            var item = new Item { ArticleGuid = article.Guid, Name = command.Name };

            itemRepository.Add(item);

            await unitOfWork.SaveChangesAsync();

            return item.Guid;
        }

        public async Task DeleteItemAsync(DeleteItemCommand command)
        {
            var item = await itemRepository.GetByGuidAsync(command.EntityGuid)
                ?? throw new InvalidOperationException("Item not found.");

            itemRepository.Remove(item);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<ItemInformation?> GetItemAsync(GetItemQuery getItemQuery)
        {
            var item = await itemRepository.GetByGuidAsync(getItemQuery.ItemGuid);

            if (item is null)
            {
                return null;
            }

            return new ItemInformation(
                item.Guid,
                item.Name,
                item.Description,
                item.CreatedAt,
                item.ParentGuid
            );
        }
    }
}
