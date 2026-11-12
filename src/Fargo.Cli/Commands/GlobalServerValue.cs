using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Resolves <c>--server</c> ahead of the main command tree so the override can be applied before
/// the HTTP client is built. Unlike the credential options, <c>--server</c> is never declared by a
/// subcommand, so a lightweight probe parse is unambiguous.
/// </summary>
public sealed class GlobalServerValue
{
    private GlobalServerValue()
    {
    }

    public Uri? Server { get; private init; }

    public string? Error { get; private init; }

    public static GlobalServerValue Parse(string[] args, GlobalOptions globalOptions)
    {
        // A dedicated option instance is used so the shared one can still be attached to the real
        // command tree without having two parents. Unmatched tokens are ignored.
        var serverOption = new Option<string?>(globalOptions.Server.Name);

        var probe = new RootCommand
        {
            TreatUnmatchedTokensAsErrors = false
        };

        probe.Options.Add(serverOption);

        var result = probe.Parse(args);

        var server = result.GetValue(serverOption);

        if (string.IsNullOrWhiteSpace(server))
        {
            return new GlobalServerValue();
        }

        if (!Uri.TryCreate(server, UriKind.Absolute, out var serverUri)
            || (serverUri.Scheme != Uri.UriSchemeHttp
                && serverUri.Scheme != Uri.UriSchemeHttps))
        {
            return new GlobalServerValue
            {
                Error = $"Invalid --server value: '{server}'. "
                        + "Provide an absolute http or https URL."
            };
        }

        return new GlobalServerValue
        {
            Server = serverUri
        };
    }
}
