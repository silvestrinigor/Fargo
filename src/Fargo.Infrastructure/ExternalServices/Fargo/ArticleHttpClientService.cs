using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.ExternalServices.Fargo;

public class ArticleHttpClientService(HttpClient httpClient) : IArticleHttpClientService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<EntityDto> CreateArticleAsync(EntityDto articleCreateDto)
    {
        var response = await httpClient.PostAsJsonAsync("/articles", articleCreateDto);

        var content = await response.Content.ReadAsStringAsync();

        var articleDto = JsonSerializer.Deserialize<EntityDto>(content, new JsonSerializerOptions
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

    public async Task<EntityDto?> GetArticleAsync(Guid guid)
    {
        var response = await httpClient.GetAsync($"/articles/{guid}");

        var content = await response.Content.ReadAsStringAsync();

        var articleDto = JsonSerializer.Deserialize<EntityDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(articleDto);

        return articleDto;
    }

    public async Task<IEnumerable<EntityDto>> GetArticlesAsync()
    {
        var response = await httpClient.GetAsync("/articles");

        var content = await response.Content.ReadAsStringAsync();

        var articlesDtos = JsonSerializer.Deserialize<IEnumerable<EntityDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(articlesDtos);

        return articlesDtos;
    }

    public Task UpdateArticleAsync(EntityDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
