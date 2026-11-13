using Fargo.Cli.Output;
using Fargo.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class RemoveUserGroupParentCommand : Command
{
    public RemoveUserGroupParentCommand(FargoApiClient client)
        : base("remove-parent", """
            Detach a user group from its parent.

            Members stop inheriting the permissions of the parent. Requires the
            ChangeUserGroupParent permission.

            Examples:
              fargo user-group remove-parent 00000000-0000-0000-0000-000000000003
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

            await client.V1.UserGroups[guid].Parent.DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("User group parent removed.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
