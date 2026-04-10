using Fargo.Sdk.Authentication;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Fargo.Web.Security;

public sealed class BrowserSdkSessionStore(IJSRuntime jsRuntime) : ISessionStore
{
    private const string Key = "fargo.auth";

    /// <summary>
    /// When true, saves the session to <c>localStorage</c> (persists across tabs/restarts).
    /// When false, saves to <c>sessionStorage</c> (cleared when the tab is closed).
    /// Set this before calling login so the SDK saves to the correct store.
    /// </summary>
    public bool RememberMe { get; set; } = true;

    public async Task SaveAsync(StoredSession session, CancellationToken cancellationToken = default)
    {
        await ClearAsync(cancellationToken);
        var storage = RememberMe ? "localStorage" : "sessionStorage";
        await jsRuntime.InvokeVoidAsync($"{storage}.setItem", cancellationToken, Key, Serialize(session));
    }

    public async Task<StoredSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var local = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, Key);
        if (!string.IsNullOrWhiteSpace(local))
        {
            return Deserialize(local);
        }

        var session = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", cancellationToken, Key);
        if (!string.IsNullOrWhiteSpace(session))
        {
            return Deserialize(session);
        }

        return null;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, Key);
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, Key);
    }

    private static string Serialize(StoredSession session)
        => JsonSerializer.Serialize(session, JsonSerializerOptions.Web);

    private static StoredSession? Deserialize(string json)
        => JsonSerializer.Deserialize<StoredSession>(json, JsonSerializerOptions.Web);
}
