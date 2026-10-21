using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class GetUserCommand : Command
{
    public GetUserCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single user.

            Examples:
              fargo user get 00000000-0000-0000-0000-000000000004
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

            var user = await client.Users[guid].GetAsync();

            if (user is null)
            {
                Console.Error.WriteLine("User not found.");
                return 1;
            }

            new DetailWriter()
                .Title("User")
                .Field("GUID", user.Guid)
                .Field("Username", user.Nameid)
                .Field("First name", user.FirstName?.ToString())
                .Field("Last name", user.LastName?.ToString())
                .Field("Description", user.Description)
                .Field("Active", user.IsActive)
                .Field("Admin", user.IsAdmin)
                .Field("Partitions", FormatGuids(user.Partitions))
                .Field("User groups", FormatGuids(user.UserGroups))
                .Write();

            return 0;
        });
    }

    private static string? FormatGuids(IEnumerable<Guid?>? guids)
    {
        if (guids is null)
        {
            return null;
        }

        var values = guids.Where(g => g.HasValue).Select(g => g!.Value.ToString()).ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }
}
