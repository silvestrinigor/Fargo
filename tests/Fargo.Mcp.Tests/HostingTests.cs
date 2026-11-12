using Fargo.ClientAuthentication;
using Fargo.ClientHttp.Extensions;
using Fargo.Mcp.Authentication;
using Fargo.Mcp.Configuration;
using Fargo.Mcp.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Mcp.Tests;

public class BearerTokenTests
{
    [Theory]
    [InlineData("Bearer abc", "abc")]
    [InlineData("bearer abc", "abc")]
    [InlineData("Bearer   abc  ", "abc")]
    public void Parse_ReturnsToken(string header, string expected) =>
        Assert.Equal(expected, BearerToken.Parse(header));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Basic abc")]
    [InlineData("Bearer ")]
    [InlineData("Bearer    ")]
    public void Parse_ReturnsNullWhenNotABearerToken(string? header) =>
        Assert.Null(BearerToken.Parse(header));
}

public class McpTransportTests
{
    [Fact]
    public void DefaultsToStdio() =>
        Assert.False(McpTransport.IsHttp([], environmentValue: null));

    [Theory]
    [InlineData("--transport", "http")]
    [InlineData("--transport", "HTTP")]
    [InlineData("--transport=http", null)]
    public void ArgumentSelectsHttp(string first, string? second) =>
        Assert.True(McpTransport.IsHttp(second is null ? [first] : [first, second], environmentValue: null));

    [Fact]
    public void EnvironmentSelectsHttp() =>
        Assert.True(McpTransport.IsHttp([], environmentValue: "http"));

    [Fact]
    public void ArgumentWinsOverEnvironment() =>
        Assert.False(McpTransport.IsHttp(["--transport", "stdio"], environmentValue: "http"));
}

public class ScopedAuthenticationTests
{
    private static ServiceProvider BuildProvider()
    {
        var options = new FargoMcpOptions { Server = new Uri("https://localhost:7001") };
        var services = new ServiceCollection();

        services.AddHttpContextAccessor();
        services.AddFargoHttpClient(options.Server);
        services.AddScopedFargoAuthentication(options);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false
        });
    }

    private static async Task<(FargoAuthenticator Authenticator, ITokenStore Store)> ResolveForRequestAsync(
        IServiceScope scope, string token)
    {
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        accessor.HttpContext = context;

        var authenticator = scope.ServiceProvider.GetRequiredService<FargoAuthenticator>();
        await authenticator.EnsureAuthenticatedAsync();

        return (authenticator, scope.ServiceProvider.GetRequiredService<ITokenStore>());
    }

    [Fact]
    public async Task EachScopeSeesOnlyItsOwnToken()
    {
        await using var provider = BuildProvider();
        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        var (_, storeA) = await ResolveForRequestAsync(scopeA, "token-a");
        var (_, storeB) = await ResolveForRequestAsync(scopeB, "token-b");

        Assert.NotSame(storeA, storeB);
        Assert.Equal("token-a", (await storeA.GetAsync())!.AccessToken);
        Assert.Equal("token-b", (await storeB.GetAsync())!.AccessToken);
    }

    [Fact]
    public async Task NewScopeDoesNotInheritPreviousToken()
    {
        await using var provider = BuildProvider();

        using (var first = provider.CreateScope())
        {
            await ResolveForRequestAsync(first, "token-a");
        }

        using var second = provider.CreateScope();
        var store = second.ServiceProvider.GetRequiredService<ITokenStore>();

        Assert.Null(await store.GetAsync());
    }

    [Fact]
    public void ScopedOptionsCarryNoCredentialsExceptTheRequestToken()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext();

        var options = scope.ServiceProvider.GetRequiredService<FargoMcpOptions>();

        Assert.False(options.HasStaticToken);
        Assert.False(options.HasPasswordCredentials);
    }
}
