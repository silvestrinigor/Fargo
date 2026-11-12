namespace Fargo.Cli.Configurations;

public sealed class FargoCliConfiguration
{
    public required Uri Server { get; set; }

    /// <summary>
    /// The real-time service URL used by <c>fargo realtime watch</c>. It is a
    /// separate service from the API, so it has its own setting.
    /// </summary>
    public Uri? RealtimeServer { get; set; }

    public string? CredentialId { get; set; } = null;
}
