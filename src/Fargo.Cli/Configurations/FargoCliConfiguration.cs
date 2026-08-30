namespace Fargo.Cli.Configurations;

public sealed class FargoCliConfiguration
{
    public required Uri Server { get; set; }

    public required string CredentialId { get; set; }
}
