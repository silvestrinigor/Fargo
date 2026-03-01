using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    public record class ArticleResponseModel(
            Guid Guid,
            Name Name,
            Description Description,
            bool IsContainer
            );
}