namespace Fargo.Application.Solicitations.Queries.ArticleQueries.GetArticleInformation
{
    public sealed record ArticleInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt
    );
}
