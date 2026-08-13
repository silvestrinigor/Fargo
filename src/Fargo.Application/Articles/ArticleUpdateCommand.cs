using Fargo.Application.Common;

namespace Fargo.Application.Articles;

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;
