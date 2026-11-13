namespace Fargo.Realtime.Contracts;

/// <summary>
/// Where the real-time hub lives and what it calls on clients.
/// </summary>
public static class FargoRealtimeHub
{
    public const string Path = "/hubs/fargo";

    /// <summary>
    /// The query parameter carrying the access token for clients that cannot
    /// send an <c>Authorization</c> header (browsers).
    /// </summary>
    public const string AccessTokenQueryParameter = "access_token";

    public const string EntityChangedMethod = nameof(IFargoRealtimeClient.EntityChanged);
}
