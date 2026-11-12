using Fargo.ClientHttp.Extensions;
using Fargo.Mcp.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Mcp.Hosting;

/// <summary>
/// Hosts the MCP server over Streamable HTTP at <c>/mcp</c>. Every request must
/// carry the caller's Fargo access token as <c>Authorization: Bearer</c>; the
/// Fargo client, token store and authenticator are scoped per request.
/// Listen addresses follow the standard ASP.NET Core settings
/// (<c>--urls</c> / <c>ASPNETCORE_URLS</c>).
/// </summary>
internal static class HttpMcpHost
{
    public const string DefaultUrl = "http://localhost:5290";

    public static async Task RunAsync(string[] args, FargoMcpOptions options)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
        {
            builder.WebHost.UseUrls(DefaultUrl);
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddFargoHttpClient(options.Server);
        builder.Services.AddScopedFargoAuthentication(options);

        builder.Services
            .AddMcpServer()
            // Stateless: no server-side session, so each request is independent
            // and always runs with the token it was sent with.
            .WithHttpTransport(http => http.Stateless = true)
            .WithToolsFromAssembly();

        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            if (BearerToken.Parse(context.Request.Headers.Authorization.ToString()) is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate = "Bearer";
                return;
            }

            await next();
        });

        app.MapMcp("/mcp");

        await app.RunAsync();
    }
}
