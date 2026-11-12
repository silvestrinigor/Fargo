namespace Fargo.RealtimeContracts;

/// <summary>
/// Where the real-time hub lives and what it calls on clients.
/// </summary>
public static class FargoRealtimeHub
{
    /// <summary>
    /// The path of the hub on the real-time service.
    /// </summary>
    public const string Path = "/hubs/fargo";

    /// <summary>
    /// The query parameter carrying the access token for clients that cannot
    /// send an <c>Authorization</c> header (browsers).
    /// </summary>
    public const string AccessTokenQueryParameter = "access_token";

    /// <summary>
    /// The client method invoked for each <see cref="EntityChangedNotification"/>.
    /// </summary>
    public const string EntityChangedMethod = nameof(IFargoRealtimeClient.EntityChanged);
}
