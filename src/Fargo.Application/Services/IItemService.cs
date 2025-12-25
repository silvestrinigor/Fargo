using Fargo.Application.Solicitations.Commands.ItemCommands;
using Fargo.Application.Solicitations.Queries.ItensQueries;
using Fargo.Application.Solicitations.Responses;

namespace Fargo.Application.Services
{
    public interface IItemService
    {
        Task<Guid> CreateItemAsync(CreateItemCommand command);
        Task<ItemInformation?> GetItemAsync(GetItemQuery getItemQuery);
        Task DeleteItemAsync(DeleteItemCommand command);
    }
}
