using Fargo.Application.IntegrationEvents;
using System.Text;

namespace Fargo.Application.Tests.IntegrationEvents;

public class IntegrationEventJsonTests
{
    private static readonly Guid EventId = Guid.Parse("6f1c2b3a-0000-7000-8000-000000000001");
    private static readonly Guid ArticleGuid = Guid.Parse("a3e9d1c2-0000-4000-8000-000000000002");
    private static readonly Guid PartitionA = Guid.Parse("10000000-0000-4000-8000-000000000001");
    private static readonly Guid PartitionB = Guid.Parse("20000000-0000-4000-8000-000000000002");
    private static readonly DateTimeOffset OccurredAt = new(2026, 11, 6, 14, 3, 22, TimeSpan.Zero);

    private const string EditedJson =
        """{"eventId":"6f1c2b3a-0000-7000-8000-000000000001","eventType":"article.edited","occurredAt":"2026-11-06T14:03:22+00:00","articleGuid":"a3e9d1c2-0000-4000-8000-000000000002","partitionGuids":["10000000-0000-4000-8000-000000000001","20000000-0000-4000-8000-000000000002"]}""";

    [Fact]
    public void Serialize_ArticleEventMessage_ShouldContainTheEnvelopeArticleGuidAndPartitions()
    {
        var message = new ArticleEventMessage(EventId, ArticleEventTypes.Edited, OccurredAt, ArticleGuid, [PartitionA, PartitionB]);

        var json = IntegrationEventJson.Serialize(message);

        Assert.Equal(EditedJson, json);
    }

    [Fact]
    public void DeserializeArticleEvent_ShouldRoundTripTheSerializedMessage()
    {
        var message = IntegrationEventJson.DeserializeArticleEvent(Encoding.UTF8.GetBytes(EditedJson));

        Assert.NotNull(message);
        Assert.Equal(EventId, message.EventId);
        Assert.Equal(ArticleEventTypes.Edited, message.EventType);
        Assert.Equal(OccurredAt, message.OccurredAt);
        Assert.Equal(ArticleGuid, message.ArticleGuid);
        Assert.Equal([PartitionA, PartitionB], message.PartitionGuids);
    }

    [Fact]
    public void DeserializeArticleEvent_WhenPartitionsAreMissing_ShouldReturnNull()
    {
        // A version 1 body: the partitions did not exist yet.
        const string versionOne =
            """{"eventId":"6f1c2b3a-0000-7000-8000-000000000001","eventType":"article.edited","occurredAt":"2026-11-06T14:03:22+00:00","articleGuid":"a3e9d1c2-0000-4000-8000-000000000002"}""";

        Assert.Null(IntegrationEventJson.DeserializeArticleEvent(Encoding.UTF8.GetBytes(versionOne)));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("""{"eventId":"6f1c2b3a-0000-7000-8000-000000000001","eventType":"article.edited","partitionGuids":[]}""")]
    public void DeserializeArticleEvent_WhenTheBodyIsNotACompleteEvent_ShouldReturnNull(string body)
    {
        Assert.Null(IntegrationEventJson.DeserializeArticleEvent(Encoding.UTF8.GetBytes(body)));
    }

    [Fact]
    public void SchemaVersion_ShouldBeTwo()
    {
        Assert.Equal(2, ArticleEventMessage.SchemaVersion);
    }

    [Theory]
    [InlineData(ArticleEventTypes.Created, "article.created")]
    [InlineData(ArticleEventTypes.Edited, "article.edited")]
    [InlineData(ArticleEventTypes.Deleted, "article.deleted")]
    public void ArticleEventTypes_ShouldBeStableRoutingKeys(string actual, string expected)
    {
        Assert.Equal(expected, actual);
    }
}
