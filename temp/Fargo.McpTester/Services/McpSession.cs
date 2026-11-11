using ModelContextProtocol.Client;

namespace Fargo.McpTester.Services;

/// <summary>
/// The MCP connection of one Blazor circuit. It is created lazily and sends the
/// user's current Fargo access token on every request, so the server
/// acts as that user. Disposed with the circuit (or on logout).
/// </summary>
public sealed class McpSession : IAsyncDisposable
{
    private readonly UserSession _user;
    private readonly string _mcpUrl;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private McpClient? _client;
    private IReadOnlyList<McpClientTool>? _tools;

    public McpSession(UserSession user, IConfiguration configuration)
    {
        _user = user;
        _mcpUrl = configuration["Mcp:Url"]
            ?? throw new InvalidOperationException("Configuration 'Mcp:Url' is required.");

        _user.Changed += OnUserChanged;
    }

    public async Task<IReadOnlyList<McpClientTool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_tools is not null)
            {
                return _tools;
            }

            _client ??= await ConnectAsync(cancellationToken);
            _tools = (await _client.ListToolsAsync(cancellationToken: cancellationToken)).ToArray();

            return _tools;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Drops the connection and the cached tool list (e.g. after logout).</summary>
    public async Task ResetAsync()
    {
        await _gate.WaitAsync();

        try
        {
            var client = _client;
            _client = null;
            _tools = null;

            if (client is not null)
            {
                await client.DisposeAsync();
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _user.Changed -= OnUserChanged;
        await ResetAsync();
    }

    private async Task<McpClient> ConnectAsync(CancellationToken cancellationToken)
    {
        // The handler asks UserSession for a (refreshed if needed) token per request.
        var httpClient = new HttpClient(new BearerTokenHandler(_user) { InnerHandler = new HttpClientHandler() });

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(_mcpUrl),
                Name = "fargo"
            },
            httpClient,
            ownsHttpClient: true);

        return await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
    }

    private void OnUserChanged()
    {
        if (!_user.IsAuthenticated)
        {
            _ = ResetAsync();
        }
    }

    private sealed class BearerTokenHandler(UserSession user) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await user.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
