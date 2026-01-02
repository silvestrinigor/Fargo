using Fargo.Application.Dtos;
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
                    article.Guid,
                    article.Name,
                    article.Description,
                    article.ShelfLife,
                    article.MaximumContainerTemperature,
                    article.MinimumContainerTemperature,
                    new MeasuresDto(                    
                        article.Measures.LengthX,
                        article.Measures.LengthY,
                        article.Measures.LengthZ,
                        article.Measures.Volume,
                        article.Measures.Mass,
                        article.Measures.Density
                        ),
                    article.Container is not null
                    ? new ContainerInformationDto(
                        article.Container.MassCapacity,
                        article.Container.VolumeCapacity,
                        article.Container.ItensQuantityCapacity,
                        article.Container.DefaultTemperature
                        )
                    : null
                    );
            }
        }
    }
}
