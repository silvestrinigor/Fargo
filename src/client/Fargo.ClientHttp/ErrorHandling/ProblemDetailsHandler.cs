using System.Net;
using System.Text.Json;

namespace Fargo.ClientHttp.ErrorHandling;

/// <summary>
/// Inspects HTTP responses and converts 401 (Unauthorized) and 403 (Forbidden)
/// into typed <see cref="FargoApiException"/> instances. When the server
/// returns an RFC 7807 problem details body, its title/detail are used for the
/// message; otherwise a sensible default is provided (for example, a bare 401
/// from the authentication middleware has no body).
/// </summary>
public sealed class ProblemDetailsHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode is not (HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden))
        {
            return response;
        }

        var problem = await TryReadProblemDetailsAsync(response, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedApiException(
                problem,
                BuildMessage(
                    problem,
                    defaultMessage:
                    "Unauthorized (401): authentication is required or your "
                    + "session has expired. Run 'fargo identity login' or "
                    + "provide --username and --password."));
        }

        throw new ForbiddenApiException(
            problem,
            BuildMessage(
                problem,
                defaultMessage:
                "Forbidden (403): you do not have permission to perform this "
                + "action."));
    }

    private static async Task<FargoProblemDetails?> TryReadProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (mediaType is not ("application/problem+json" or "application/json"))
        {
            return null;
        }

        try
        {
            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            if (stream.CanSeek && stream.Length == 0)
            {
                return null;
            }

            return await JsonSerializer
                .DeserializeAsync<FargoProblemDetails>(
                    stream,
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildMessage(
        FargoProblemDetails? problem,
        string defaultMessage)
    {
        if (problem is null)
        {
            return defaultMessage;
        }

        var hasTitle = !string.IsNullOrWhiteSpace(problem.Title);
        var hasDetail = !string.IsNullOrWhiteSpace(problem.Detail);

        // The server's "detail" carries the specific, actionable explanation
        // (e.g. "The provided credentials are invalid."). A bare response from
        // the authentication/authorization middleware only has a generic status
        // title (e.g. "Unauthorized") with no detail, in which case the
        // actionable default guidance is more helpful.
        if (!hasDetail)
        {
            return defaultMessage;
        }

        return hasTitle
            ? $"{problem.Title} {problem.Detail}"
            : problem.Detail!;
    }
}
