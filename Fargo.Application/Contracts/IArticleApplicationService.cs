using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts;

public interface IArticleApplicationService
{
    Task<ArticleDto?> GetArticleAsync(Guid guid);
    Task<IEnumerable<ArticleDto>> GetArticlesAsync();
    Task<ArticleDto> CreateArticleAsync(ArticleDto articleCreateDto);
    Task DeleteArticleAsync(Guid guid);
    Task UpdateArticleAsync(ArticleDto articleUpdateDto);
}
