using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemUpdateCommand(
            Guid ItemGuid,
            ItemUpdateModel Item
            ) : ICommand;

    public sealed class ItemUpdateCommandHandler(
            IUnitOfWork unitOfWork
            ) : ICommandHandler<ItemUpdateCommand>
    {
        public async Task Handle(ItemUpdateCommand command, CancellationToken cancellationToken = default)
        {
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}