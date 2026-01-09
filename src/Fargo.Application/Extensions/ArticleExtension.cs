using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class ArticleExtension
    {
        extension(Article article)
        {
            public ArticleDto ToDto()
                => new(
                    article.Guid,
                    article.Name,
                    article.Description,
                    article.IsContainer
                    );
        }
    }
}
