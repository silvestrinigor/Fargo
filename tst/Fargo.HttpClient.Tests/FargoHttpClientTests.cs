using Fargo.HttpContracts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Fargo.HttpClient.Tests;

public sealed class FargoHttpClientTests
{
    private static readonly JsonSerializerOptions JsonOptions = FargoHttpJsonSerializerOptions.Create();

    [Fact]
    public async Task GetManyAsync_Should_SendQueryAndBearerToken()
    {
        var partitionGuid = Guid.NewGuid();
        var handler = new RecordingHandler(JsonResponse(Array.Empty<ArticleDto>()));
        var client = CreateClient(handler, bearerToken: "access-token");
        var temporalAsOf = new DateTimeOffset(2026, 5, 24, 10, 15, 30, TimeSpan.Zero);

        await client.Articles.GetManyAsync(new FargoListQuery(
            temporalAsOf,
            Page: 2,
            Limit: 50,
            ChildOfAnyOfThesePartitions: [partitionGuid],
            NotChildOfAnyPartition: true));

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.StartsWith("/articles/?", request.PathAndQuery, StringComparison.Ordinal);
        Assert.Contains("temporalAsOfDateTime=", request.PathAndQuery);
        Assert.Contains("page=2", request.PathAndQuery);
        Assert.Contains("limit=50", request.PathAndQuery);
        Assert.Contains($"childOfAnyOfThesePartitions={partitionGuid:D}", request.PathAndQuery);
        Assert.Contains("notChildOfAnyPartition=true", request.PathAndQuery);
        Assert.Equal("Bearer", request.Authorization?.Scheme);
        Assert.Equal("access-token", request.Authorization?.Parameter);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenNotFound()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = CreateClient(handler);

        var result = await client.Articles.GetAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_WhenNoContent()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = CreateClient(handler);

        var result = await client.Items.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_Should_PostJsonAndReadGuid()
    {
        var createdGuid = Guid.NewGuid();
        var handler = new RecordingHandler(JsonResponse(createdGuid));
        var client = CreateClient(handler);

        var result = await client.Partitions.CreateAsync(new PartitionCreateRequest("Main"));

        Assert.Equal(createdGuid, result);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/partitions/", request.PathAndQuery);
        Assert.Contains("\"name\":\"Main\"", request.Body);
    }

    [Fact]
    public async Task GetByBarcodeAsync_Should_EncodeBarcodeRouteValue()
    {
        var handler = new RecordingHandler(JsonResponse<ArticleDto?>(null));
        var client = CreateClient(handler);

        await client.Articles.GetByBarcodeAsync("7891234567895", BarcodeFormat.Ean13);

        var request = Assert.Single(handler.Requests);
        Assert.StartsWith("/articles/7891234567895%3AEan13", request.PathAndQuery, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NonSuccessResponse_Should_ThrowProblemDetailsException()
    {
        var problem = new FargoProblemDetailsDto
        {
            Status = 400,
            Title = "Invalid request",
            Detail = "The request is invalid.",
            Type = "request/invalid",
            Instance = "/identity/login",
            TraceId = "trace"
        };
        var handler = new RecordingHandler(JsonResponse(problem, HttpStatusCode.BadRequest, "application/problem+json"));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<FargoHttpApiException>(
            () => client.Identity.LoginAsync(new LoginRequest("admin", "password")));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.ProblemDetails);
        Assert.Equal("request/invalid", exception.ProblemDetails.Type);
        Assert.Contains("The request is invalid.", exception.Message);
    }

    private static IFargoHttpClient CreateClient(
        RecordingHandler handler,
        string? bearerToken = null)
    {
        var services = new ServiceCollection();

        services
            .AddFargoHttpClient(options =>
            {
                options.BaseAddress = new Uri("https://api.example.test");
                options.BearerToken = bearerToken;
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider().GetRequiredService<IFargoHttpClient>();
    }

    private static HttpResponseMessage JsonResponse<T>(
        T value,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string mediaType = "application/json")
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(value, JsonOptions),
                Encoding.UTF8,
                mediaType)
        };

        return response;
    }

    private sealed class RecordingHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri?.PathAndQuery ?? string.Empty,
                request.Headers.Authorization,
                body));

            return responses.Count == 0
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        string PathAndQuery,
        AuthenticationHeaderValue? Authorization,
        string Body
    );
}
