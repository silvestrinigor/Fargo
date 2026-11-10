using Fargo.ServiceOutbox;
using Fargo.ServiceOutbox.Options;

namespace Fargo.Infrastructure.Tests.Outbox;

public class OutboxRelayServiceTests
{
    private static readonly OutboxOptions Options = new()
    {
        PollInterval = TimeSpan.FromSeconds(1),
        MaxRetryDelay = TimeSpan.FromSeconds(10),
    };

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(4, 8)]
    [InlineData(5, 10)]
    [InlineData(1000, 10)]
    public void GetRetryDelay_ShouldDoubleUpToTheMaximum(int consecutiveFailures, int expectedSeconds)
    {
        var delay = OutboxRelayService.GetRetryDelay(Options, consecutiveFailures);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), delay);
    }
}
