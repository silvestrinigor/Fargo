using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Fargo.Web.Api;

internal static class FargoApiErrors
{
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
        }

        if (problem is not null && !string.IsNullOrWhiteSpace(problem.Detail))
        {
            throw new HttpRequestException(problem.Detail, null, response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException("Your session is no longer valid.", null, response.StatusCode);
        }

        response.EnsureSuccessStatusCode();
    }
}
