using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    public record ArticleCreateModel(
        Name Name,
        Description? Description = null,
        bool IsContainer = false);
}
