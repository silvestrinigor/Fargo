using Fargo.Application.Contracts;
using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Dtos;

namespace Fargo.Application.Services;

public class ArticleApplicationHttpClientService(IArticleHttpClientService articleApplicationExternalService) : IArticleApplicationService
{
    private readonly IArticleHttpClientService articleApplicationExternalService = articleApplicationExternalService;

    public async Task<ArticleDto> CreateArticleAsync(ArticleDto articleCreateDto)
    {
        return await articleApplicationExternalService.CreateArticleAsync(articleCreateDto);
    }

    public async Task<ArticleDto?> GetArticleAsync(Guid guid)
    {
        return await articleApplicationExternalService.GetArticleAsync(guid);
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesAsync()
    {
        return await articleApplicationExternalService.GetArticlesAsync();
    }
    public async Task DeleteArticleAsync(Guid guid)
    {
        await articleApplicationExternalService.DeleteArticleAsync(guid);
    }

    public async Task UpdateArticleAsync(ArticleDto articleUpdateDto)
    {
        await articleApplicationExternalService.UpdateArticleAsync(articleUpdateDto);
    }
}
