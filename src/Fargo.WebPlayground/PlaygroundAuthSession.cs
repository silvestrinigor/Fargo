using Fargo.HttpClient;
using Fargo.Application.Shared.Identity;
using Fargo.Core.Shared.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Fargo.WebPlayground;

public sealed class PlaygroundAuthSession(
    IJSRuntime jsRuntime,
    IFargoIdentityClient identityClient)
{
    public const string AuthStorageKey = "fargo.playground.auth";

    private const string LoginStateStorageKey = "fargo.playground.auth.state";
    private static readonly JsonSerializerOptions JsonOptions = FargoHttpJsonSerializerOptions.Create();

    public AuthResult? CurrentAuth { get; private set; }

    public bool IsAuthenticated
        => CurrentAuth is not null &&
           CurrentAuth.ExpiresAt > DateTimeOffset.UtcNow;

    public async ValueTask<AuthResult?> LoadAsync()
    {
        var rawAuth = await GetPersistedAuthAsync();

        if (string.IsNullOrWhiteSpace(rawAuth))
        {
            CurrentAuth = null;
            return null;
        }

        try
        {
            CurrentAuth = JsonSerializer.Deserialize<AuthResult>(rawAuth, JsonOptions);
        }
        catch (JsonException)
        {
            await ClearAsync();
            return null;
        }

        if (CurrentAuth is null)
        {
            await ClearAsync();
            return null;
        }

        if (!IsAuthenticated)
        {
            return await TryRefreshAsync(CurrentAuth.RefreshToken);
        }

        return CurrentAuth;
    }

    public async ValueTask StoreAsync(AuthResult auth)
    {
        CurrentAuth = auth;
        var rawAuth = JsonSerializer.Serialize(auth, JsonOptions);

        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            AuthStorageKey,
            rawAuth);
        await jsRuntime.InvokeVoidAsync(
            "sessionStorage.removeItem",
            AuthStorageKey);
    }

    public async ValueTask ClearAsync()
    {
        CurrentAuth = null;

        await jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            AuthStorageKey);
        await jsRuntime.InvokeVoidAsync(
            "sessionStorage.removeItem",
            AuthStorageKey);
    }

    public ValueTask SetLoginStateAsync(string state)
        => jsRuntime.InvokeVoidAsync(
            "sessionStorage.setItem",
            LoginStateStorageKey,
            state);

    public ValueTask<string?> GetLoginStateAsync()
        => jsRuntime.InvokeAsync<string?>(
            "sessionStorage.getItem",
            LoginStateStorageKey);

    public ValueTask ClearLoginStateAsync()
        => jsRuntime.InvokeVoidAsync(
            "sessionStorage.removeItem",
            LoginStateStorageKey);

    public static bool TryDecodeAuthPayload(
        string? payload,
        out AuthResult? auth)
    {
        auth = null;

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(payload);
            auth = JsonSerializer.Deserialize<AuthResult>(bytes, JsonOptions);

            return auth is not null;
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or JsonException)
        {
            auth = null;
            return false;
        }
    }

    private async ValueTask<string?> GetPersistedAuthAsync()
    {
        var rawAuth = await jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            AuthStorageKey);

        if (!string.IsNullOrWhiteSpace(rawAuth))
        {
            return rawAuth;
        }

        var legacyRawAuth = await jsRuntime.InvokeAsync<string?>(
            "sessionStorage.getItem",
            AuthStorageKey);

        if (!string.IsNullOrWhiteSpace(legacyRawAuth))
        {
            await jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                AuthStorageKey,
                legacyRawAuth);
            await jsRuntime.InvokeVoidAsync(
                "sessionStorage.removeItem",
                AuthStorageKey);
        }

        return legacyRawAuth;
    }

    private async ValueTask<AuthResult?> TryRefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await ClearAsync();
            return null;
        }

        try
        {
            var refreshedAuth = await identityClient.RefreshAsync(
                new RefreshDto(new Token(refreshToken)));

            await StoreAsync(refreshedAuth);
            return CurrentAuth;
        }
        catch (Exception exception) when (
            exception is FargoHttpApiException or HttpRequestException or TaskCanceledException)
        {
            await ClearAsync();
            return null;
        }
    }
}
