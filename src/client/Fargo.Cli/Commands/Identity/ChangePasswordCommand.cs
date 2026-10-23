using Fargo.Cli.Authentication;
using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class ChangePasswordCommand : Command
{
    public ChangePasswordCommand(FargoApiClient client)
        : base("change-password", """
            Change the password for an account.

            If --current-password or --new-password are omitted you will be
            prompted to type them securely (the input is not echoed).

            Examples:
              fargo identity change-password --username admin
              fargo identity change-password --username admin --current-password old --new-password new
            """)
    {
        var nameidOption = new Option<string>("--username")
        {
            Description = "Username (nameid) of the account.",
            Required = true
        };

        var currentPasswordOption = new Option<string?>("--current-password")
        {
            Description = "Current password. If omitted, you will be prompted securely."
        };

        var newPasswordOption = new Option<string?>("--new-password")
        {
            Description = "New password. If omitted, you will be prompted securely."
        };

        Add(nameidOption);
        Add(currentPasswordOption);
        Add(newPasswordOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var nameid = parseResult.GetValue(nameidOption)!;
            var currentPassword = parseResult.GetValue(currentPasswordOption);
            var newPassword = parseResult.GetValue(newPasswordOption);

            if (string.IsNullOrEmpty(currentPassword))
            {
                currentPassword = PasswordPrompt.Read("Current password: ");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                newPassword = PasswordPrompt.Read("New password: ");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                Console.Error.WriteLine("Both current and new passwords are required.");
                return 1;
            }

            await client.Identity.Password.PutAsync(new IdentityPasswordUpdateDto
            {
                Nameid = nameid,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            }, cancellationToken: cancellationToken);

            StatusWriter.Success("Password changed successfully.", ("User", nameid));

            return 0;
        });
    }
}
