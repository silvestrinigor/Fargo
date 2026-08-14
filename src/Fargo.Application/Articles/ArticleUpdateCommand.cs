using Fargo.Application.Common;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents a command to update an existing article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article to update.</param>
/// <param name="Article">The data containing the changes to apply to the article.</param>
public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;
