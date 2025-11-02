using Fargo.Application.Dtos;
using Fargo.Core.Entities;

namespace Fargo.Application.Extensions;

public static class ArticleExtension
{
    public static ArticleDto ToDto(this Article article)
    {
        return new ArticleDto() { Name = article.Name, Description = article.Description, Guid = article.Guid};
    }

    public static Article Update(this Article article, ArticleDto articleDto)
    {
        return article;
    }
}
