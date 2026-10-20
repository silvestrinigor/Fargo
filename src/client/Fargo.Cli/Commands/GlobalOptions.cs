using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Options that are available on every command in the CLI. They are registered
/// as recursive options on the root command so they can be supplied after any
/// subcommand (e.g. <c>fargo items list --server https://localhost:7563</c>).
/// </summary>
public sealed class GlobalOptions
{
    public GlobalOptions()
    {
        Server = new Option<string?>("--server")
        {
            Description =
                "The Fargo server URL to connect to (e.g. https://localhost:7563). "
                + "Overrides the configured server for this invocation.",
            Recursive = true
        };

        Username = new Option<string?>("--username")
        {
            Description =
                "Username to authenticate with for this invocation. "
                + "Must be used together with --password.",
            Recursive = true
        };

        Password = new Option<string?>("--password")
        {
            Description =
                "Password to authenticate with for this invocation. "
                + "Must be used together with --username.",
            Recursive = true
        };
    }

    public Option<string?> Server { get; }

    public Option<string?> Username { get; }

    public Option<string?> Password { get; }

    public void AddToRoot(RootCommand root)
    {
        root.Options.Add(Server);
        root.Options.Add(Username);
        root.Options.Add(Password);
    }
}
