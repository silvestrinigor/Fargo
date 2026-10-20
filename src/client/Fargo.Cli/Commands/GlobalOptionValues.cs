using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// The resolved values of the global options, parsed ahead of the main command
/// tree so that <c>--server</c> can be applied before the HTTP client is built
/// and credentials can be exchanged for a token up front.
/// </summary>
public sealed class GlobalOptionValues
{
    private GlobalOptionValues()
    {
    }

    public Uri? Server { get; private init; }

    public string? Username { get; private init; }

    public string? Password { get; private init; }

    public string? Error { get; private init; }

    public static GlobalOptionValues Parse(string[] args, GlobalOptions globalOptions)
    {
        // A throwaway root containing only the global options. Unmatched tokens
        // (the actual command and its options) are ignored so we can read the
        // global values regardless of which command was selected. Dedicated
        // option instances are used so the shared options can be attached to the
        // real command tree without being owned by two parents.
        var serverOption = new Option<string?>(globalOptions.Server.Name);
        var usernameOption = new Option<string?>(globalOptions.Username.Name);
        var passwordOption = new Option<string?>(globalOptions.Password.Name);

        var probe = new RootCommand
        {
            TreatUnmatchedTokensAsErrors = false
        };

        probe.Options.Add(serverOption);
        probe.Options.Add(usernameOption);
        probe.Options.Add(passwordOption);

        var result = probe.Parse(args);

        var server = result.GetValue(serverOption);
        var username = result.GetValue(usernameOption);
        var password = result.GetValue(passwordOption);

        Uri? serverUri = null;

        if (!string.IsNullOrWhiteSpace(server))
        {
            if (!Uri.TryCreate(server, UriKind.Absolute, out serverUri)
                || (serverUri.Scheme != Uri.UriSchemeHttp
                    && serverUri.Scheme != Uri.UriSchemeHttps))
            {
                return new GlobalOptionValues
                {
                    Error = $"Invalid --server value: '{server}'. "
                            + "Provide an absolute http or https URL."
                };
            }
        }

        var hasUsername = !string.IsNullOrWhiteSpace(username);
        var hasPassword = !string.IsNullOrWhiteSpace(password);

        // Ad-hoc authentication only kicks in when BOTH credentials are present.
        // A lone --username or --password is left for the selected command to
        // handle (e.g. `identity login --username x` prompts for the password,
        // and `users create --username x` treats it as the new user's name).
        return new GlobalOptionValues
        {
            Server = serverUri,
            Username = hasUsername && hasPassword ? username : null,
            Password = hasUsername && hasPassword ? password : null
        };
    }
}
