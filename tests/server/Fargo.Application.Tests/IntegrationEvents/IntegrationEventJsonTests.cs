using Fargo.Application.IntegrationEvents;

namespace Fargo.Application.Tests.IntegrationEvents;

public class IntegrationEventJsonTests
{
    [Fact]
    public void Serialize_ArticleEventMessage_ShouldContainOnlyEnvelopeAndArticleGuid()
    {
        var message = new ArticleEventMessage(
            Guid.Parse("6f1c2b3a-0000-7000-8000-000000000001"),
            ArticleEventTypes.Edited,
            new DateTimeOffset(2026, 11, 6, 14, 3, 22, TimeSpan.Zero),
            Guid.Parse("a3e9d1c2-0000-4000-8000-000000000002"));

        var json = IntegrationEventJson.Serialize(message);

        Assert.Equal(
            """{"eventId":"6f1c2b3a-0000-7000-8000-000000000001","eventType":"article.edited","occurredAt":"2026-11-06T14:03:22+00:00","articleGuid":"a3e9d1c2-0000-4000-8000-000000000002"}""",
            json);
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
