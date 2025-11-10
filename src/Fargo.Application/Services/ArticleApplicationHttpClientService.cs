using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;

namespace Fargo.Application.Services;

public class ArticleApplicationHttpClientService(IArticleHttpClientService articleApplicationExternalService) : IArticleApplicationService
{
    private readonly IArticleHttpClientService articleApplicationExternalService = articleApplicationExternalService;

    public async Task<EntityDto> CreateArticleAsync(EntityCreateDto articleCreateDto)
    {
        return await articleApplicationExternalService.CreateArticleAsync(articleCreateDto);
    }

    public async Task<EntityDto?> GetArticleAsync(Guid guid)
    {
        return await articleApplicationExternalService.GetArticleAsync(guid);
    }

    public async Task<IEnumerable<EntityDto>> GetArticlesAsync()
    {
        return await articleApplicationExternalService.GetArticlesAsync();
    }
    public async Task DeleteArticleAsync(Guid guid)
    {
        await articleApplicationExternalService.DeleteArticleAsync(guid);
    }

    public async Task UpdateArticleAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto)
    {
        await articleApplicationExternalService.UpdateArticleAsync(containerGuid, articleUpdateDto);
    }

    public Task<IEnumerable<Guid>> GetArticlesGuidsAsync()
    {
        throw new NotImplementedException();
    }
}
