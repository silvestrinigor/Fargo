namespace Fargo.Cli.Authentication;

/// <summary>
/// Holds the server the current invocation is actually talking to (the
/// configured server, or the <c>--server</c> override when provided). The token
/// store uses this to bind persisted tokens to the server they were issued for,
/// so a token obtained against one server is never sent to another.
/// </summary>
public sealed class ActiveServerContext
{
    public Uri? Server { get; set; }
}
