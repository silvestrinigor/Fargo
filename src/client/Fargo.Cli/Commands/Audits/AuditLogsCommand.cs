using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class AuditLogsCommand : Command
{
    public AuditLogsCommand(FargoApiClient client)
        : base("audit", "Manage audit logs")
    {
        Add(new ListAuditLogsCommand(client));
    }
}
