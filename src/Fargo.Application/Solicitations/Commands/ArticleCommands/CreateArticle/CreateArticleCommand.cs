using Fargo.Application.Interfaces.Solicitations.Commands;

namespace Fargo.Application.Solicitations.Commands.ArticleCommands.CreateArticle
{
    public sealed record CreateArticleCommand(string? Name, string? Description) : ICommand<CreateArticleCommand, Task>;
}
