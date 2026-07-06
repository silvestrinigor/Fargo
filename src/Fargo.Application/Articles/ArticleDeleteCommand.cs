namespace Fargo.Application.Articles;

/// <summary>
/// Command to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand;
