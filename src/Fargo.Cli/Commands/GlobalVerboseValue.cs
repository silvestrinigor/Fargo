using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Resolves <c>--verbose</c> (<c>-v</c>) ahead of the main command tree. Errors raised during
/// early token refresh or ad-hoc sign-in happen before <see cref="ParseResult.InvokeAsync"/>, so
/// the flag must be known up front via a lightweight probe parse.
/// </summary>
public static class GlobalVerboseValue
{
    public static bool Parse(string[] args, GlobalOptions globalOptions)
    {
        // A dedicated option instance is used so the shared one can still be attached to the real
        // command tree without having two parents. Unmatched tokens are ignored.
        var verboseOption = new Option<bool>(
            globalOptions.Verbose.Name,
            [.. globalOptions.Verbose.Aliases]);

        var probe = new RootCommand
        {
            TreatUnmatchedTokensAsErrors = false
        };

        probe.Options.Add(verboseOption);

        return probe.Parse(args).GetValue(verboseOption);
    }
}
