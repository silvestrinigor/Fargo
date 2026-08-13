using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class RootCommandFactory
{
    public static RootCommand Create()
    {
        var root = new RootCommand("Fargo command-line client.")
        {
            SessionCommands.CreateConnectCommand(),
            SessionCommands.CreateLoginCommand(),
            SessionCommands.CreateWhoAmICommand()
        };

        return root;
    }
}
