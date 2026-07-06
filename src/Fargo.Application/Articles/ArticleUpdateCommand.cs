using Fargo.Application.Shared.Articles;

namespace Fargo.Application.Articles;

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article) : ICommand;
