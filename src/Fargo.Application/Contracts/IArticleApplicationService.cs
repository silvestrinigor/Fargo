using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts;

public interface IArticleApplicationService
{
    Task<EntityDto?> GetArticleAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetArticlesAsync();
    Task<EntityDto> CreateArticleAsync(EntityDto articleCreateDto);
    Task DeleteArticleAsync(Guid guid);
    Task UpdateArticleAsync(EntityDto articleUpdateDto);
}
