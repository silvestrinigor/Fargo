using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    public sealed record ArticleUpdateModel(
            Name? Name = null,
            Description? Description = null
            );
}