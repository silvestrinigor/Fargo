using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Cli.Configurations;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
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

// The persistent store writes to the OS credential cache; the session store
// wraps it so that ad-hoc `--username`/`--password` credentials only live for
// the current invocation.
serviceCollection.AddSingleton<FargoCliTokenStore>();
serviceCollection.AddSingleton<SessionTokenStore>();
serviceCollection.AddSingleton<ITokenStore>(
    sp => sp.GetRequiredService<SessionTokenStore>());

var serviceProvider = serviceCollection.BuildServiceProvider();

var globalOptions = new GlobalOptions();

// Resolve the global options first so --server can be applied before the HTTP
// client is created, and credentials can be exchanged for a token up front.
var globals = GlobalOptionValues.Parse(args, globalOptions);

if (globals.Error is not null)
{
    Console.Error.WriteLine(globals.Error);
    return 1;
}

if (globals.Server is not null)
{
    serviceProvider
        .GetRequiredService<ClientConnectionContext>()
        .BaseAddressOverride = globals.Server;
}

var client = serviceProvider.GetRequiredService<FargoApiClient>();

if (globals.Username is not null && globals.Password is not null)
{
    var authResult = await client.Identity.Login.PostAsync(new IdentityLoginDto
    {
        Nameid = globals.Username,
        Password = globals.Password
    });

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

var root = await CommandFactory.Create(serviceProvider, globalOptions);

return await root.Parse(args).InvokeAsync();
