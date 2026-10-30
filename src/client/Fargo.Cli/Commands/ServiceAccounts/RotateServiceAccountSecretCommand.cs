using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class RotateServiceAccountSecretCommand : Command
{
    public RotateServiceAccountSecretCommand(FargoApiClient client)
        : base("rotate-secret", """
            Rotate a service account's secret. The new secret is printed EXACTLY
            ONCE and all tokens previously issued to the account stop validating.

            Examples:
              fargo service-account rotate-secret 00000000-0000-0000-0000-000000000010
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var rotated = await client.V1.ServiceAccounts[guid].RotateSecret
                .PostAsync(cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Secret rotated. Store the new secret now - it will not be shown again.",
                ("GUID", rotated?.Guid?.ToString()),
                ("Client ID", rotated?.ClientId),
                ("Client secret", rotated?.ClientSecret));

            return 0;
        });
    }
}
