using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class DeleteUserGroupCommand : Command
{
    public DeleteUserGroupCommand(FargoApiClient client)
        : base("delete", """
            Delete a user group by GUID.

            Examples:
              fargo user-group delete 00000000-0000-0000-0000-000000000003
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.UserGroups[guid].DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("User group deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
