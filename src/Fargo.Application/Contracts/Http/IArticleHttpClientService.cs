using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts.Http;

public interface IArticleHttpClientService
{
    Task<EntityDto?> GetArticleAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetArticlesAsync();
    Task<IEnumerable<Guid>> GetArticlesGuidsAsync();
    Task<EntityDto> CreateArticleAsync(EntityCreateDto articleCreateDto);
    Task DeleteArticleAsync(Guid guid);
    Task UpdateArticleAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto);
}
