using Fargo.Application.Interfaces.Solicitations.Commands;

namespace Fargo.Application.Solicitations.Commands.ArticleItemCommands.CreateArticleItem
{
    public sealed record CreateArticleItemCommand(string Name, Guid Article) : ICommand<CreateArticleItemCommand, Task>;
}
