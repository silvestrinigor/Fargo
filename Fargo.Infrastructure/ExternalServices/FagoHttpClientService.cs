using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.ExternalServices;

public class FagoHttpClientService(HttpClient httpClient) : IArticleHttpClientService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<ArticleDto> CreateArticleAsync(ArticleDto articleCreateDto)
    {
        var response = await httpClient.PostAsJsonAsync("/articles", articleCreateDto);

        var content = await response.Content.ReadAsStringAsync();

        var articleDto = JsonSerializer.Deserialize<ArticleDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(articleDto);

        return articleDto;
    }

    public Task DeleteArticleAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public async Task<ArticleDto?> GetArticleAsync(Guid guid)
    {
        var response = await httpClient.GetAsync($"/articles/{guid}");

        var content = await response.Content.ReadAsStringAsync();

        var articleDto = JsonSerializer.Deserialize<ArticleDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(articleDto);

        return articleDto;
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesAsync()
    {
        var response = await httpClient.GetAsync("/articles");

        var content = await response.Content.ReadAsStringAsync();

        var articlesDtos = JsonSerializer.Deserialize<IEnumerable<ArticleDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(articlesDtos);

        return articlesDtos;
    }

    public Task UpdateArticleAsync(ArticleDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
