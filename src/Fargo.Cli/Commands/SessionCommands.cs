using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class SessionCommands
{
    public static Command CreateConnectCommand()
    {
        var serverArgument = new Argument<string>("server")
        {
            Description = "Fargo server address."
        };

        var command = new Command("connect", "Connect to a Fargo server.")
        {
            serverArgument
        };

        command.SetAction(parseResult =>
        {
            var server = parseResult.GetValue(serverArgument);

            Console.WriteLine($"TODO: Connect to {server}");

            return 0;
        });

        return command;
    }

    public static Command CreateLoginCommand()
    {
        var command = new Command("login", "Authenticate with the Fargo server.");

        command.SetAction(_ =>
        {
            Console.WriteLine("TODO: Authenticate.");

            return 0;
        });

        return command;
    }

    public static Command CreateWhoAmICommand()
    {
        var command = new Command("whoami", "Show the currently authenticated user.");

        command.SetAction(_ =>
        {
            Console.WriteLine("TODO: Get current user.");
        });

        return command;
    }
}
