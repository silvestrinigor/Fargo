using Fargo.Application.Interfaces.Solicitations.Queries;

namespace Fargo.Application.Solicitations.Queries.ArticleQueries.GetArticleInformation
{
    public sealed record GetArticleQuery(Guid ArticleGuid) : IQuery<GetArticleQuery, Task<ArticleInformation?>>;
}
