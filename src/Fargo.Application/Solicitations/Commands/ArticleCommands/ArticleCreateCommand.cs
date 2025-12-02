namespace Fargo.Application.Solicitations.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
        string? Name, 
        string? Description
        );
}
