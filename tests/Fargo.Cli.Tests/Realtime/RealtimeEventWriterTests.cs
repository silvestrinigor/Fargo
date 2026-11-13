using Fargo.Cli.Commands.Config;
using Fargo.Cli.Commands.Realtime;
using Fargo.Realtime.Contracts;
using System.Text.Json;

namespace Fargo.Cli.Tests.Realtime;

public class RealtimeEventWriterTests
{
    private static readonly EntityChangedNotification Notification = new(
        EntityChangedNotification.EntityTypes.Article,
        Guid.Parse("a3e9d1c2-0000-4000-8000-000000000002"),
        EntityChangedNotification.ChangeTypes.Edited,
        Guid.Parse("6f1c2b3a-0000-7000-8000-000000000001"),
        new DateTimeOffset(2026, 11, 6, 11, 3, 22, TimeSpan.FromHours(-3)));

    [Fact]
    public void FormatText_ShouldWriteOneAlignedLineInUtc()
    {
        Assert.Equal(
            "2026-11-06 14:03:22Z  edited   article  a3e9d1c2-0000-4000-8000-000000000002",
            RealtimeEventWriter.FormatText(Notification));
    }

    [Fact]
    public void FormatJson_ShouldWriteOneLineWithTheWireFieldNames()
    {
        var json = RealtimeEventWriter.FormatJson(Notification);

        Assert.DoesNotContain('\n', json);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            ["entityType", "guid", "changeType", "eventId", "occurredAt"],
            document.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.Equal("article", document.RootElement.GetProperty("entityType").GetString());
        Assert.Equal("edited", document.RootElement.GetProperty("changeType").GetString());
    }

    [Theory]
    [InlineData("https://localhost:7571", true)]
    [InlineData("http://realtime.example/fargo", true)]
    [InlineData("ftp://realtime.example", false)]
    [InlineData("localhost:7571", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RealtimeServerUrl_ShouldAcceptOnlyAbsoluteHttpUrls(string? value, bool expected)
    {
        Assert.Equal(expected, RealtimeServerUrl.TryParse(value, out _));
    }
}
