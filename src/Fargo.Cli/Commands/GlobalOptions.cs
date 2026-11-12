using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Options shared across commands.
///
/// <para>
/// <c>--server</c> is recursive on the root, so it applies to every command
/// (including the identity commands, which still need to target a server).
/// </para>
///
/// <para>
/// <c>--username</c>/<c>--password</c> provide ad-hoc authentication for a
/// single invocation. They are attached only to the resource command groups,
/// not to the identity group: the identity commands either take their own
/// credential options (login, change-password) or operate on the stored session
/// (logout, refresh), so the global credentials would be meaningless there.
/// </para>
///
/// <para>
/// <c>--verbose</c>/<c>-v</c> is recursive on the root and enables detailed
/// error diagnostics (exception types, inner exceptions, stack traces).
/// </para>
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

    /// <summary>Adds the always-available <c>--server</c> option to the root.</summary>
    public void AddServerToRoot(RootCommand root)
    {
        root.Options.Add(Server);
    }

    /// <summary>Adds the always-available <c>--verbose</c> option to the root.</summary>
    public void AddVerboseToRoot(RootCommand root)
    {
        root.Options.Add(Verbose);
    }

    /// <summary>
    /// Adds the ad-hoc <c>--username</c>/<c>--password</c> options to a command
    /// group so they are available (recursively) on that group's subcommands.
    /// </summary>
    public void AddCredentialsTo(Command command)
    {
        command.Options.Add(Username);
        command.Options.Add(Password);
    }
}
