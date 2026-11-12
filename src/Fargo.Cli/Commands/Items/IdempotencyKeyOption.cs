using Fargo.ClientHttp;
using Microsoft.Kiota.Abstractions;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

/// <summary>
/// The <c>--idempotency-key</c> option of the commands that create stock, so a retry cannot create it twice.
/// </summary>
/// <remarks>
/// Without the option the command still sends a fresh key, which protects its own automatic retries (for
/// example after a rate-limit response). To make a re-run of the command safe, as in a script that may run
/// twice, pass the same key each time.
/// </remarks>
internal sealed class IdempotencyKeyOption
{
    public Option<Guid?> Option { get; } = new("--idempotency-key")
    {
        Description = "A GUID that makes the request safe to retry: sending the same key again returns the first result instead of repeating it. Use a new key for a different request. A new key is generated when omitted."
    };

    public Action<RequestConfiguration<DefaultQueryParameters>> Configure(ParseResult parseResult)
        => IdempotencyKeys.Header(parseResult.GetValue(Option) ?? Guid.NewGuid());
}