using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class GetUserGroupCommand : Command
{
    public GetUserGroupCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single user group.

            Examples:
              fargo user-groups get 00000000-0000-0000-0000-000000000003
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var group = await client.UserGroups[guid].GetAsync();

            if (group is null)
            {
                Console.Error.WriteLine("User group not found.");
                return 1;
            }

            new DetailWriter()
                .Title("User group")
                .Field("GUID", group.Guid)
                .Field("Name", group.Nameid)
                .Field("Description", group.Description)
                .Field("Active", group.IsActive)
                .Field("Admin group", group.IsAdminUserGroup)
                .Field("Parent partition", group.ParentPartition)
                .Write();

            return 0;
        });
    }
}
