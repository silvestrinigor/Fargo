using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Net;

namespace Fargo.Cli;

public static class Application
{
    public static RootCommand Create()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddFargoHttpClient(
            new Uri("http://localhost:5000"));

        builder.Services.AddSingleton<CredentialCache>();

        builder.Services.AddSingleton<ITokenStore, FargoCliTokenStore>();

        var host = builder.Build();

        return CommandFactory.Create(host.Services);
    }
}
