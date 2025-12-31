using Fargo.Application.Dtos.EntitiesDtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class ArticleDtoExtensions
    {
        extension(Article article)
        {
            public ArticleDto ToDto()
            {
                return new ArticleDto(
                    article.NameDescriptionInformation?.Name,
                    article.NameDescriptionInformation?.Description,
                    article.ShelfLife,
                    article.LengthX,
                    article.LengthY,
                    article.LengthZ,
                    article.Mass,
                    article.Volume,
                    article.Density
                );
            }
        }
    }
}
