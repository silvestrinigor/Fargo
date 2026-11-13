using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class UpdateUserGroupCommand : Command
{
    public UpdateUserGroupCommand(FargoApiClient client)
        : base("update", """
            Update the name or description of an existing user group. Only the options you pass are changed.

            Activation, permissions, partitions and the parent group have their own
            commands: set-active, set-permissions, set-partitions, set-parent and remove-parent.

            Examples:
              fargo user-group update 00000000-0000-0000-0000-000000000003 --name editors
              fargo user-group update 00000000-0000-0000-0000-000000000003 --description "Content editors"
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        var nameOption = new Option<string?>("--name")
        {
            Description = "New name identifier (nameid) for the user group."
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "New description."
        };

        Add(guidArgument);
        Add(nameOption);
        Add(descriptionOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new UserGroupUpdateDto
            {
                Nameid = parseResult.GetValue(nameOption),
                Description = parseResult.GetValue(descriptionOption)
            };

            await client.V1.UserGroups[guid].PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("User group updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
