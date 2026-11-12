namespace Fargo.ClientHttp.Factories;

/// <summary>
/// Holds a per-process override for the server the client connects to. When set
/// before the first <see cref="FargoClientFactory.GetClient"/> call, it replaces
/// the base address configured at startup. This lets callers point the CLI at a
/// different server for a single invocation (e.g. via a <c>--server</c> option).
/// </summary>
public sealed class ClientConnectionContext
{
    public Uri? BaseAddressOverride { get; set; }
}
