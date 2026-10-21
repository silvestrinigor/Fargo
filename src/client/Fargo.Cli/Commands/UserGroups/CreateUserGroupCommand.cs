using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class CreateUserGroupCommand : Command
{
    public CreateUserGroupCommand(FargoApiClient client)
        : base("create", """
            Create a new user group, optionally nested under a parent group.

            Examples:
              fargo user-group create --name warehouse-ops
              fargo user-group create --name warehouse-ops --parent 00000000-0000-0000-0000-000000000003
            """)
    {
        var nameidOption = new Option<string>("--name")
        {
            Description = "Name identifier (nameid) for the user group.",
            Required = true
        };

        var isActiveOption = new Option<bool>("--active")
        {
            Description = "Whether the group is active. Defaults to true.",
            DefaultValueFactory = _ => true
        };

        var parentOption = new Option<Guid?>("--parent")
        {
            Description = "GUID of the parent user group."
        };

        Add(nameidOption);
        Add(isActiveOption);
        Add(parentOption);

        SetAction(async parseResult =>
        {
            var nameid = parseResult.GetValue(nameidOption)!;
            var isActive = parseResult.GetValue(isActiveOption);
            var parent = parseResult.GetValue(parentOption);

            var dto = new UserGroupCreateDto
            {
                Nameid = nameid,
                IsActive = isActive,
                ParentUserGroup = parent
            };

            var guid = await client.UserGroups.PostAsync(dto);

            StatusWriter.Success(
                "User group created.",
                ("GUID", guid?.ToString()),
                ("Name", nameid));

            return 0;
        });
    }
}
