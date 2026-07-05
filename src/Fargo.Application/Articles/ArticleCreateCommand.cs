using Fargo.Application.Shared.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Command to create a article.
/// </summary>
public sealed record ArticleCreateCommand(
    ArticleCreateDto Article
) : ICommand<Guid>;
