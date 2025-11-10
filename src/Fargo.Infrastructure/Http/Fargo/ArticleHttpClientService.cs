using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;
using System;
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

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EntityDto>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task<IEnumerable<EntityDto>> GetArticlesAsync()
    {
        var response = await httpClient.GetAsync("/articles/");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<EntityDto>>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task<IEnumerable<Guid>> GetArticlesGuidsAsync()
    {
        var response = await httpClient.GetAsync("/articles/guids");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<Guid>>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task UpdateArticleAsync(Guid articleGuid, EntityUpdateDto articleUpdateDto)
    {
        var response = await httpClient.PatchAsJsonAsync($"/articles/{articleGuid}", articleUpdateDto);

        response.EnsureSuccessStatusCode();
    }
}
