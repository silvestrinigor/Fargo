namespace Fargo.Cli.Configurations;

public sealed class FargoCliConfiguration
{
    public required Uri Server { get; set; }

    public string CredentialId { get; set; } = string.Empty;
}
