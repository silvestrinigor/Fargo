using System.Net;
using System.Text;

namespace Fargo.Client.Tests;

internal sealed class FakeRefreshHandler : HttpMessageHandler
{
    private int callCount;

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public string ResponseBody { get; set; } =
        """{"accessToken":"new-access","refreshToken":"new-refresh","expiresAt":"2030-01-01T00:00:00+00:00"}""";

    /// <summary>
    /// When set, each request waits for it, so concurrent callers overlap.
    /// </summary>
    public TaskCompletionSource? Gate { get; set; }

    public int CallCount => Volatile.Read(ref callCount);

    public List<(Uri Uri, string Body)> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref callCount);

        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);

        lock (Requests)
        {
            Requests.Add((request.RequestUri!, body));
        }

        if (Gate is { } gate)
        {
            await gate.Task.WaitAsync(cancellationToken);
        }

        return new HttpResponseMessage(StatusCode)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
        };
    }
}
