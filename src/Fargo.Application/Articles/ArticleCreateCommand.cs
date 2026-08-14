using Fargo.Application.Common;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents a command to create a new article.
/// </summary>
/// <param name="Create">
/// The data required to create the article.
/// </param>
public sealed record ArticleCreateCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;
