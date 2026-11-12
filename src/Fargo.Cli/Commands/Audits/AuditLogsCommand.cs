using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class AuditLogsCommand : Command
{
    public AuditLogsCommand(FargoApiClient client)
        : base("audit", "View audit logs - a record of actions performed in the system.")
    {
        Add(new ListAuditLogsCommand(client));
        Add(new GetAuditLogCommand(client));
    }
}
