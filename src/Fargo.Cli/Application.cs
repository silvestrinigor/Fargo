using Fargo.Cli.Commands;
using Fargo.Http.Client.Extensions;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

namespace Fargo.Cli;

public static class Application
{
    public static RootCommand Create()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddFargoHttpClient(
            new Uri("http://localhost:5000"));

        var host = builder.Build();

        return CommandFactory.Create(host.Services);
    }
}
