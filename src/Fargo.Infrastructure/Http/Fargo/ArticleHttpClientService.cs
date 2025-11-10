using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.Http.Fargo;

public class ArticleHttpClientService(HttpClient httpClient) : IArticleHttpClientService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EntityDto> CreateArticleAsync(EntityCreateDto articleCreateDto)
    {
        var response = await httpClient.PostAsJsonAsync("/articles", articleCreateDto);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EntityDto>(jsonSerializerOptions) 
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task DeleteArticleAsync(Guid guid)
    {
        var response = await httpClient.DeleteAsync($"/articles/{guid}");

        response.EnsureSuccessStatusCode();
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

    public Task<IEnumerable<Guid>> GetArticlesGuidsAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateArticleAsync(Guid articleGuid, EntityUpdateDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
