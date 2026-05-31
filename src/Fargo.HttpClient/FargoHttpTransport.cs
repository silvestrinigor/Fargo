using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Fargo.HttpClient;

internal sealed class FargoHttpTransport(System.Net.Http.HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = FargoHttpJsonSerializerOptions.Create();

    public async Task<TResponse> SendRequiredAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The Fargo API returned an empty response body.");
    }

    public async Task<TResponse?> SendNullableAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TResponse>> SendCollectionAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return [];
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TResponse>>(JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task SendNoContentAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public static string BuildPath(string path, IEnumerable<KeyValuePair<string, string?>> query)
    {
        var builder = new StringBuilder(path);
        var separator = '?';

        foreach (var (key, value) in query)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.Append(separator);
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            separator = '&';
        }

        return builder.ToString();
    }

    public static IEnumerable<KeyValuePair<string, string?>> ListQuery(FargoListQuery? query)
    {
        if (query is null)
        {
            yield break;
        }

        yield return new KeyValuePair<string, string?>(
            "temporalAsOfDateTime",
            query.TemporalAsOf?.ToString("O", CultureInfo.InvariantCulture));
        yield return new KeyValuePair<string, string?>(
            "page",
            query.Page?.ToString(CultureInfo.InvariantCulture));
        yield return new KeyValuePair<string, string?>(
            "limit",
            query.Limit?.ToString(CultureInfo.InvariantCulture));

        if (query.ChildOfAnyOfThesePartitions is not null)
        {
            foreach (var partitionGuid in query.ChildOfAnyOfThesePartitions)
            {
                yield return new KeyValuePair<string, string?>(
                    "childOfAnyOfThesePartitions",
                    partitionGuid.ToString("D"));
            }
        }

        yield return new KeyValuePair<string, string?>(
            "notChildOfAnyPartition",
            query.NotChildOfAnyPartition?.ToString().ToLowerInvariant());
    }

    public static IEnumerable<KeyValuePair<string, string?>> SingleQuery(DateTimeOffset? temporalAsOf)
    {
        yield return new KeyValuePair<string, string?>(
            "temporalAsOf",
            temporalAsOf?.ToString("O", CultureInfo.InvariantCulture));
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        FargoProblemDetailsDto? problemDetails = null;

        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            try
            {
                problemDetails = JsonSerializer.Deserialize<FargoProblemDetailsDto>(rawBody, JsonOptions);
            }
            catch (JsonException)
            {
                problemDetails = null;
            }
        }

        throw new FargoHttpApiException(
            response.StatusCode,
            response.ReasonPhrase,
            rawBody,
            problemDetails);
    }
}
