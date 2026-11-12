using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class SetUserGroupParentCommand : Command
{
    public SetUserGroupParentCommand(FargoApiClient client)
        : base("set-parent", """
            Place a user group under a parent group.

            Members of the group inherit the permissions of the parent and of its
            ancestors, so you must hold all of them. This is a separate operation from
            update and requires the ChangeUserGroupParent permission. Use 'remove-parent'
            to detach the group.

            Examples:
              fargo user-group set-parent 00000000-0000-0000-0000-000000000003 --parent 00000000-0000-0000-0000-000000000009
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        var parentOption = new Option<Guid>("--parent")
        {
            Description = "GUID of the new parent user group.",
            Required = true
        };

        Add(guidArgument);
        Add(parentOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var parent = parseResult.GetValue(parentOption);

            await client.V1.UserGroups[guid].Parent.PutAsync(
                new ParentUserGroupDto { ParentUserGroupGuid = parent },
                cancellationToken: cancellationToken);

            StatusWriter.Success("User group parent set.", ("GUID", guid.ToString()), ("Parent", parent.ToString()));

            return 0;
        });
    }
}