using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class UpdateUserGroupCommand : Command
{
    public UpdateUserGroupCommand(FargoApiClient client)
        : base("update", "Update an existing user group")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        var isActiveOption = new Option<bool?>("--active")
        {
            Description = "Set user group active state."
        };

        var parentOption = new Option<Guid?>("--parent")
        {
            Description = "GUID of the parent user group."
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

            Console.WriteLine("User group updated.");

            return 0;
        });
    }
}
