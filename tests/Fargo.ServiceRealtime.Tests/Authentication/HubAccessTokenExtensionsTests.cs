using Fargo.ServiceRealtime.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.ServiceRealtime.Tests.Authentication;

public class HubAccessTokenExtensionsTests
{
    private static async Task<string?> ReadTokenAsync(string path, string queryString, string? authorizationHeader = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddHubAccessTokenFromQueryString();

        await using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = provider };
        httpContext.Request.Path = path;
        httpContext.Request.QueryString = new QueryString(queryString);

        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var context = new MessageReceivedContext(httpContext, scheme, options)
        {
            // A token already read from the Authorization header is kept as is.
            Token = authorizationHeader
        };

        await options.Events.OnMessageReceived(context);

        return context.Token;
    }

    [Fact]
    public async Task OnMessageReceived_ForAHubPath_ShouldReadTheQueryToken()
    {
        Assert.Equal("abc", await ReadTokenAsync("/hubs/fargo", "?access_token=abc"));
    }

    [Fact]
    public async Task OnMessageReceived_ForTheNegotiateRequest_ShouldReadTheQueryToken()
    {
        Assert.Equal("abc", await ReadTokenAsync("/hubs/fargo/negotiate", "?negotiateVersion=1&access_token=abc"));
    }

    [Fact]
    public async Task OnMessageReceived_OutsideTheHubs_ShouldIgnoreTheQueryToken()
    {
        Assert.Null(await ReadTokenAsync("/health", "?access_token=abc"));
        Assert.Null(await ReadTokenAsync("/hubsfake", "?access_token=abc"));
    }

    [Fact]
    public async Task OnMessageReceived_WhenATokenWasAlreadyRead_ShouldKeepIt()
    {
        Assert.Equal("header", await ReadTokenAsync("/hubs/fargo", "?access_token=abc", "header"));
    }

    [Fact]
    public async Task OnMessageReceived_WhenTheQueryTokenIsEmpty_ShouldReadNothing()
    {
        Assert.Null(await ReadTokenAsync("/hubs/fargo", "?access_token="));
    }
}
