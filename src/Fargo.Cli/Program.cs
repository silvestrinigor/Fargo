using Fargo.Cli.Commands;
using Fargo.Cli.Authentication;
using Fargo.Cli.Configurations;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<CredentialCache>();

builder.Services.AddSingleton<ITokenStore, FargoCliTokenStore>();

builder.Services.AddSingleton<IFargoCliConfigurationStore, FargoCliConfigurationStore>();

builder.Services.AddFargoHttpClient(new Uri("https://localhost:7563"));

var host = builder.Build();

var root = CommandFactory.Create(host.Services);

await root.Parse(args).InvokeAsync();
