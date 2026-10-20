using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class UpdateUserGroupCommand : Command
{
    public UpdateUserGroupCommand(FargoApiClient client)
        : base("update", """
            Update an existing user group. Only the options you pass are changed.

            Examples:
              fargo user-groups update 00000000-0000-0000-0000-000000000003 --active false
              fargo user-groups update 00000000-0000-0000-0000-000000000003 --remove-parent
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        var isActiveOption = new Option<bool?>("--active")
        {
            Description = "Set the user group active state (true or false)."
        };

        var parentOption = new Option<Guid?>("--parent")
        {
            Description = "New parent user group GUID."
        };

        var removeParentOption = new Option<bool>("--remove-parent")
        {
            Description = "Remove the parent user group association."
        };

        Add(guidArgument);
        Add(isActiveOption);
        Add(parentOption);
        Add(removeParentOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var isActive = parseResult.GetValue(isActiveOption);
            var parent = parseResult.GetValue(parentOption);
            var removeParent = parseResult.GetValue(removeParentOption);

            var dto = new UserGroupUpdateDto
            {
                IsActive = isActive,
                ParentUserGroup = parent,
                RemoveParentUserGroup = removeParent ? true : null
            };

            await client.UserGroups[guid].PutAsync(dto);

            StatusWriter.Success("User group updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
