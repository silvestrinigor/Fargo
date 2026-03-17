// BrowserClientSessionStore.cs
using Microsoft.JSInterop;

namespace Fargo.Web.Security;

public sealed class BrowserClientSessionStore(IJSRuntime jsRuntime) : IClientSessionStore
{
    private const string LocalKey = "fargo.auth";
    private const string SessionKey = "fargo.auth";

    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task SaveAsync(AuthenticatedSession session, bool rememberMe)
    {
        await ClearAsync();

        var storage = rememberMe ? "localStorage" : "sessionStorage";
        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", rememberMe ? LocalKey : SessionKey, Serialize(session));
    }

    public async Task<AuthenticatedSession?> GetAsync()
    {
        var local = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LocalKey);
        if (!string.IsNullOrWhiteSpace(local))
        {
            return Deserialize(local);
        }

        var session = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionKey);
        if (!string.IsNullOrWhiteSpace(session))
        {
            return Deserialize(session);
        }

        return null;
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LocalKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionKey);
    }

    private static string Serialize(AuthenticatedSession session)
        => System.Text.Json.JsonSerializer.Serialize(session);

    private static AuthenticatedSession? Deserialize(string json)
        => System.Text.Json.JsonSerializer.Deserialize<AuthenticatedSession>(json);
}
