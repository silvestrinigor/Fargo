using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class DeleteUserCommand : Command
{
    public DeleteUserCommand(FargoApiClient client)
        : base("delete", """
            Delete a user by GUID.

            Examples:
              fargo user delete 00000000-0000-0000-0000-000000000004
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Users[guid].DeleteAsync();

            StatusWriter.Success("User deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
