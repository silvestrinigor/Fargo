using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class GetUserGroupCommand : Command
{
    public GetUserGroupCommand(FargoApiClient client)
        : base("get", "Get a single user group by GUID")
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

            Console.WriteLine($"GUID:    {group.Guid}");
            Console.WriteLine($"Name:    {group.Nameid}");
            Console.WriteLine($"Active:  {group.IsActive}");

            return 0;
        });
    }
}
