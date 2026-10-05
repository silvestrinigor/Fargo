using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class GetUserCommand : Command
{
    public GetUserCommand(FargoApiClient client)
        : base("get", "Get a single user by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var user = await client.Users[guid].GetAsync();

            if (user is null)
            {
                Console.Error.WriteLine("User not found.");
                return 1;
            }

            Console.WriteLine($"GUID:      {user.Guid}");
            Console.WriteLine($"Username:  {user.Nameid}");
            Console.WriteLine($"Active:    {user.IsActive}");
            Console.WriteLine($"Admin:     {user.IsAdmin}");

            return 0;
        });
    }
}
