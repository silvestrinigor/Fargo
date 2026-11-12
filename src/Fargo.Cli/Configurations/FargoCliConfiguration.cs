namespace Fargo.Cli.Configurations;

public sealed class FargoCliConfiguration
{
    public required Uri Server { get; set; }

    /// <summary>
    /// Separate from <see cref="Server"/> because the real-time service is not part of the API.
    /// </summary>
    public Uri? RealtimeServer { get; set; }

    public string? CredentialId { get; set; } = null;
}
