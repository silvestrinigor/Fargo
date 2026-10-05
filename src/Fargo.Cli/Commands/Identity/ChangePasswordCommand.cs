using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class ChangePasswordCommand : Command
{
    public ChangePasswordCommand(FargoApiClient client)
        : base("change-password", "Change the password for the current user")
    {
        var nameidOption = new Option<string>("--username")
        {
            Description = "Username (nameid) of the account.",
            Required = true
        };

        var currentPasswordOption = new Option<string>("--current-password")
        {
            Description = "Current password.",
            Required = true
        };

        var newPasswordOption = new Option<string>("--new-password")
        {
            Description = "New password.",
            Required = true
        };

        Add(nameidOption);
        Add(currentPasswordOption);
        Add(newPasswordOption);

        SetAction(async parseResult =>
        {
            var nameid = parseResult.GetValue(nameidOption)!;
            var currentPassword = parseResult.GetValue(currentPasswordOption)!;
            var newPassword = parseResult.GetValue(newPasswordOption)!;

            await client.Identity.Password.PutAsync(new IdentityPasswordUpdateDto
            {
                Nameid = nameid,
                CurrentPassword = currentPassword,
                NewPassword = new UntypedString(newPassword)
            });

            Console.WriteLine("Password changed successfully.");

            return 0;
        });
    }
}
