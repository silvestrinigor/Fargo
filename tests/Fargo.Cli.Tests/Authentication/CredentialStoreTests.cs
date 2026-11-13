using Fargo.Cli.Authentication;
using Fargo.Cli.Commands;
using Fargo.Cli.ErrorHandling;
using System.CommandLine;

namespace Fargo.Cli.Tests.Authentication;

public class CredentialStoreTests
{
    [Fact]
    public void AMissingNativeLibrary_ShouldSurfaceAsAnUnavailableStore()
    {
        // What ktsu.CredentialCache throws on Linux without libsecret.
        var native = new TypeInitializationException("Schema", new DllNotFoundException("libsecret-1.so.0"));

        var exception = Assert.Throws<CredentialStoreUnavailableException>(
            () => FargoCliTokenStore.UseCredentialStore<bool>(() => throw native));

        Assert.Same(native, exception.InnerException);
        Assert.Contains("--username and --password", exception.Message);
    }

    [Fact]
    public void Cancellation_ShouldNotBeReportedAsAnUnavailableStore()
    {
        Assert.Throws<OperationCanceledException>(
            () => FargoCliTokenStore.UseCredentialStore<bool>(() => throw new OperationCanceledException()));
    }

    [Fact]
    public void AnUnavailableStore_ShouldBeReportedWithoutAStackTrace()
    {
        var exception = new CredentialStoreUnavailableException(new DllNotFoundException("libsecret-1.so.0"));

        Assert.True(ApiErrorReporter.CanReport(exception));
    }

    [Theory]
    [InlineData("config set-server --url https://localhost:7563/", true)]
    [InlineData("identity login", true)]
    [InlineData("article list", false)]
    public void CommandGroups_ShouldMatchTheInvokedCommandOrItsParents(string commandLine, bool expected)
    {
        var root = new RootCommand
        {
            new Command("config") { new Command("set-server") { new Option<string>("--url") } },
            new Command("identity") { new Command("login") },
            new Command("article") { new Command("list") },
        };

        var result = root.Parse(commandLine);

        Assert.Equal(expected, CommandGroups.IsIn(result, "identity", "config"));
    }
}
