using System.CommandLine;

namespace Fargo.Cli.Commands;

/// <summary>
/// Resolves the <c>--verbose</c> (<c>-v</c>) global flag ahead of the main
/// command tree so detailed diagnostics can be enabled before any network or
/// authentication work runs. Errors raised during early token refresh or ad-hoc
/// sign-in happen before <see cref="ParseResult.InvokeAsync"/>, so the flag must
/// be known up front via a lightweight probe parse.
/// </summary>
public static class GlobalVerboseValue
{
    public static bool Parse(string[] args, GlobalOptions globalOptions)
    {
        // A throwaway root containing only the verbose flag. Unmatched tokens
        // (the actual command and its other options) are ignored. A dedicated
        // option instance is used so the shared option can still be attached to
        // the real command tree without being owned by two parents.
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
