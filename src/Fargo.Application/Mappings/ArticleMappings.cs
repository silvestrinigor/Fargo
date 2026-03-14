using System.Linq.Expressions;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Application.Mappings
{
    public static class ArticleMappings
    {
        public static readonly Expression<Func<Article, ArticleInformation>> InformationProjection =
            a => new ArticleInformation(
                a.Guid,
                a.Name,
                a.Description
            );

        public static ArticleInformation ToInformation(this Article a) =>
            new(
                a.Guid,
                a.Name,
                a.Description
            );
    }
}