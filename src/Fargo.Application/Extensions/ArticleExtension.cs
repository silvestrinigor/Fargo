using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Models;

namespace Fargo.Application.Extensions
{
    public static class ArticleExtension
    {
        extension(Article article)
        {
            public ArticleDto ToDto()
            {
                return new ArticleDto(
                    article.Guid,
                    article.Name,
                    article.Description
                    );
            }
        }
    }
}
