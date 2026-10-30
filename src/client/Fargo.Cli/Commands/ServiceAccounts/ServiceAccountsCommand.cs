using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class ServiceAccountsCommand : Command
{
    public ServiceAccountsCommand(FargoApiClient client)
        : base("service-account", "Manage service accounts - non-interactive machine identities that authenticate with a client id and secret.")
    {
        Add(new ListServiceAccountsCommand(client));
        Add(new GetServiceAccountCommand(client));
        Add(new CreateServiceAccountCommand(client));
        Add(new UpdateServiceAccountCommand(client));
        Add(new RotateServiceAccountSecretCommand(client));
        Add(new DeleteServiceAccountCommand(client));
    }
}
