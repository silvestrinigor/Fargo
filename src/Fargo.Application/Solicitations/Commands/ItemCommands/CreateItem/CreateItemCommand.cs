using Fargo.Application.Interfaces.Solicitations.Commands;

namespace Fargo.Application.Solicitations.Commands.ItemCommands.CreateItem
{
    public sealed record CreateItemCommand(string? Name, string? Description, Guid ArticleGuid) : ICommand<CreateItemCommand, Task>;
}
