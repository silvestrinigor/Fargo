using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using Microsoft.Kiota.Abstractions;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class UpdateUserCommand : Command
{
    public UpdateUserCommand(FargoApiClient client)
        : base("update", "Update an existing user")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        var usernameOption = new Option<string?>("--username")
        {
            Description = "New username (nameid) for the user."
        };

        var firstNameOption = new Option<string?>("--first-name")
        {
            Description = "New first name."
        };

        var lastNameOption = new Option<string?>("--last-name")
        {
            Description = "New last name."
        };

        var isActiveOption = new Option<bool?>("--active")
        {
            Description = "Set user active state."
        };

        Add(guidArgument);
        Add(usernameOption);
        Add(firstNameOption);
        Add(lastNameOption);
        Add(isActiveOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var username = parseResult.GetValue(usernameOption);
            var firstName = parseResult.GetValue(firstNameOption);
            var lastName = parseResult.GetValue(lastNameOption);
            var isActive = parseResult.GetValue(isActiveOption);

            var dto = new UserUpdateDto();

            if (username is not null) dto.Nameid = new UntypedString(username);
            if (firstName is not null) dto.FirstName = new UntypedString(firstName);
            if (lastName is not null) dto.LastName = new UntypedString(lastName);
            if (isActive.HasValue) dto.IsActive = isActive;

            await client.Users[guid].PutAsync(dto);

            Console.WriteLine("User updated.");

            return 0;
        });
    }
}
