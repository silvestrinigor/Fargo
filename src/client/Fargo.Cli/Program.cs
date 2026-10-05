using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Cli.Configurations;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<CredentialCache>();

serviceCollection.AddFargoHttpClient(new Uri("https://localhost:7563"));

serviceCollection.AddSingleton<ITokenStore, FargoCliTokenStore>();

serviceCollection.AddSingleton<IFargoCliConfigurationStore, FargoCliConfigurationStore>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var root = await CommandFactory.Create(serviceProvider);

await root.Parse(args).InvokeAsync();
