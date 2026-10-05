using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class DeleteUserGroupCommand : Command
{
    public DeleteUserGroupCommand(FargoApiClient client)
        : base("delete", "Delete a user group by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.UserGroups[guid].DeleteAsync();

            Console.WriteLine("User group deleted.");

            return 0;
        });
    }
}
