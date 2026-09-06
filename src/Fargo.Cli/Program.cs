using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Cli.Configurations;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<CredentialCache>();

builder.Services.AddFargoHttpClient(new Uri("https://localhost:7563"));

builder.Services.AddSingleton<ITokenStore, FargoCliTokenStore>();

builder.Services.AddSingleton<IFargoCliConfigurationStore, FargoCliConfigurationStore>();

var host = builder.Build();

var root = await CommandFactory.Create(host.Services);

await root.Parse(args).InvokeAsync();
