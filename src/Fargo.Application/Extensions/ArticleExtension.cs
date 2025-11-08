using Fargo.Application.Dtos;
using Fargo.Core.Entities;

namespace Fargo.Application.Extensions;

public static class ArticleExtension
{
    public static EntityDto ToDto(this Article article)
    {
        return new EntityDto() { Name = article.Name, Description = article.Description, Guid = article.Guid};
    }

    public static Article Update(this Article article, EntityDto articleDto)
    {
        return article;
    }
}
