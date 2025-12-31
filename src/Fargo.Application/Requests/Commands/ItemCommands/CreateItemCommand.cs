namespace Fargo.Application.Solicitations.Commands.ItemCommands
{
    public sealed record CreateItemCommand(string? Name, string? Description, Guid ArticleGuid);
}
