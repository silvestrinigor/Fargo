using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class CommandFactory
{
    public static RootCommand Create(IServiceProvider services)
    {
        var root = new RootCommand("Fargo CLI");

        return root;
    }
}
