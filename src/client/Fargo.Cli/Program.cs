using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Cli.Configurations;
using Fargo.Cli.ErrorHandling;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.ErrorHandling;
using Fargo.ClientHttp.Extensions;
using Fargo.ClientHttp.Factories;
using Fargo.ClientHttp.Models;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Net;

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<CredentialCache>();

var configurationStore = new FargoCliConfigurationStore();

serviceCollection.AddSingleton<IFargoCliConfigurationStore>(configurationStore);

var configuration = configurationStore.Load();

serviceCollection.AddFargoHttpClient(configuration.Server);

// Tracks the server this invocation actually targets so tokens stay bound to it.
var activeServerContext = new ActiveServerContext { Server = configuration.Server };
serviceCollection.AddSingleton(activeServerContext);

// The persistent store writes to the OS credential cache; the session store
// wraps it so that ad-hoc `--username`/`--password` credentials only live for
// the current invocation.
serviceCollection.AddSingleton<FargoCliTokenStore>();
serviceCollection.AddSingleton<SessionTokenStore>();
serviceCollection.AddSingleton<ITokenStore>(
    sp => sp.GetRequiredService<SessionTokenStore>());

var serviceProvider = serviceCollection.BuildServiceProvider();

var globalOptions = new GlobalOptions();

// Resolve --server first so the override can be applied before the HTTP client
// is created when the command tree is built.
var serverValue = GlobalServerValue.Parse(args, globalOptions);

if (serverValue.Error is not null)
{
    Console.Error.WriteLine(serverValue.Error);
    return 1;
}

if (serverValue.Server is not null)
{
    serviceProvider
        .GetRequiredService<ClientConnectionContext>()
        .BaseAddressOverride = serverValue.Server;

    // Bind tokens to the overridden server for this invocation.
    activeServerContext.Server = serverValue.Server;
}

var root = await CommandFactory.Create(serviceProvider, globalOptions);

var parseResult = root.Parse(args);

// Help, version, and parse-error invocations do not call a command that needs
// the API client, so skip any credential/token work (and the network) for them.
// This also keeps `--help` usable in environments without a credential backend.
var actionTypeName = parseResult.Action?.GetType().Name;
var skipAuthentication =
    parseResult.Action is System.CommandLine.Help.HelpAction
    || actionTypeName == "VersionAction"
    || parseResult.Errors.Count > 0;

// Read the ad-hoc credentials from the real parse result. Commands that declare
// their own --username/--password (login, users create, change-password) shadow
// the recursive global options, so these come back null for them and the global
// sign-in is correctly skipped.
var username = skipAuthentication ? null : parseResult.GetValue(globalOptions.Username);
var password = skipAuthentication ? null : parseResult.GetValue(globalOptions.Password);

if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
{
    var client = serviceProvider.GetRequiredService<FargoApiClient>();

    IdentityAuthResultDto? authResult;

    try
    {
        authResult = await client.Identity.Login.PostAsync(new IdentityLoginDto
        {
            Nameid = username,
            Password = password
        });
    }
    catch (FargoApiException ex)
    {
        return ApiErrorReporter.Report(ex);
    }

    if (authResult?.AccessToken is null
        || authResult.RefreshToken is null
        || authResult.ExpiresAt is null)
    {
        Console.Error.WriteLine(
            "Authentication with the provided --username/--password failed.");
        return 1;
    }

    serviceProvider
        .GetRequiredService<SessionTokenStore>()
        .SetSessionOverride(new AuthTokens(
            authResult.AccessToken,
            authResult.RefreshToken,
            authResult.ExpiresAt.Value));
}
else if (!skipAuthentication && !IsIdentityCommand(parseResult))
{
    // For any non-identity command, refresh the stored token up front if it has
    // expired, so the request carries a valid access token. The identity
    // commands manage authentication themselves and are skipped.
    var client = serviceProvider.GetRequiredService<FargoApiClient>();
    var tokenStore = serviceProvider.GetRequiredService<ITokenStore>();

    try
    {
        await TokenRefresher.EnsureValidAsync(client, tokenStore);
    }
    catch (FargoApiException ex)
    {
        return ApiErrorReporter.Report(ex);
    }
}

// Disable the built-in exception handler so API errors (401/403, etc.) can be
// rendered as concise messages instead of stack traces.
var invocationConfiguration = new InvocationConfiguration
{
    EnableDefaultExceptionHandler = false
};

try
{
    return await parseResult.InvokeAsync(invocationConfiguration);
}
catch (FargoApiException ex)
{
    return ApiErrorReporter.Report(ex);
}

// Returns true when the invoked command belongs to the "identity" group, whose
// commands handle authentication themselves and must not be auto-refreshed.
static bool IsIdentityCommand(System.CommandLine.ParseResult result)
{
    var command = result.CommandResult.Command;

    while (command is not null)
    {
        if (string.Equals(command.Name, "identity", StringComparison.Ordinal))
        {
            return true;
        }

        command = (command as System.CommandLine.Command)?.Parents
            .OfType<System.CommandLine.Command>()
            .FirstOrDefault();
    }

    return false;
}
