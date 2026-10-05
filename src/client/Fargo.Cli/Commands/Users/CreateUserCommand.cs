using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class CreateUserCommand : Command
{
    public CreateUserCommand(FargoApiClient client)
        : base("create", "Create a new user")
    {
        var usernameOption = new Option<string>("--username")
        {
            Description = "Username (nameid) for the new user.",
            Required = true
        };

        var passwordOption = new Option<string>("--password")
        {
            Description = "Initial password for the new user.",
            Required = true
        };

        var firstNameOption = new Option<string?>("--first-name")
        {
            Description = "First name of the user."
        };

        var lastNameOption = new Option<string?>("--last-name")
        {
            Description = "Last name of the user."
        };

        var isActiveOption = new Option<bool>("--active")
        {
            Description = "Whether the user is active. Defaults to true.",
            DefaultValueFactory = x => true
        };

        Add(usernameOption);
        Add(passwordOption);
        Add(firstNameOption);
        Add(lastNameOption);
        Add(isActiveOption);

        SetAction(async parseResult =>
        {
            var username = parseResult.GetValue(usernameOption)!;
            var password = parseResult.GetValue(passwordOption)!;
            var firstName = parseResult.GetValue(firstNameOption);
            var lastName = parseResult.GetValue(lastNameOption);
            var isActive = parseResult.GetValue(isActiveOption);

            var dto = new UserCreateDto
            {
                Nameid = username,
                IsActive = isActive,
                Authentication = new UserAuthenticationCreateDto
                {
                    Password = password
                }
            };

            if (firstName is not null)
            {
                dto.FirstName = firstName;
            }

            if (lastName is not null)
            {
                dto.LastName = lastName;
            }

            var guid = await client.Users.PostAsync(dto);

            Console.WriteLine($"User created: {guid}");

            return 0;
        });
    }
}
