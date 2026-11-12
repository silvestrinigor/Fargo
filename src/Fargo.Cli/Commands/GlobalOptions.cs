using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Options shared across commands.
/// </summary>
/// <remarks>
/// <c>--server</c> and <c>--verbose</c> are attached to the root, so they apply to every command,
/// identity commands included. <c>--username</c>/<c>--password</c> sign in for a single invocation
/// and are attached only to the resource command groups, not to the identity group: its commands
/// either take their own credential options (login, change-password) or operate on the stored
/// session (logout), so the global credentials would be meaningless there.
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
                + "Provide together with --password to sign in just for this command.",
            Recursive = true
        };

        Password = new Option<string?>("--password")
        {
            Description =
                "Password to authenticate with for this invocation. "
                + "Provide together with --username to sign in just for this command.",
            Recursive = true
        };

        Verbose = new Option<bool>("--verbose", "-v")
        {
            Description =
                "Show detailed diagnostics for errors, including exception "
                + "types, inner exceptions, and stack traces.",
            Recursive = true
        };
    }

    public Option<string?> Server { get; }

    public Option<string?> Username { get; }

    public Option<string?> Password { get; }

    public Option<bool> Verbose { get; }

    public void AddServerToRoot(RootCommand root)
    {
        root.Options.Add(Server);
    }

    public void AddVerboseToRoot(RootCommand root)
    {
        root.Options.Add(Verbose);
    }

    public void AddCredentialsTo(Command command)
    {
        command.Options.Add(Username);
        command.Options.Add(Password);
    }
}
