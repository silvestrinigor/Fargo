using System.Text.Json;

namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// Serializes integration event messages to the JSON format published to the
/// broker (camelCase property names, no indentation).
/// </summary>
public static class IntegrationEventJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);

    public static string Serialize(ArticleEventMessage message)
        => JsonSerializer.Serialize(message, Options);

    /// <summary>
    /// Returns null when the body is not a complete article event (malformed JSON, or a required field
    /// such as <see cref="ArticleEventMessage.PartitionGuids"/> is missing).
    /// </summary>
    public static ArticleEventMessage? DeserializeArticleEvent(ReadOnlySpan<byte> body)
    {
        ArticleEventMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<ArticleEventMessage>(body, Options);
        }
        catch (JsonException)
        {
            return null;
        }

        // System.Text.Json leaves absent properties at their default value
        // instead of failing, so check the required ones explicitly.
        if (message is null ||
            message.EventId == Guid.Empty ||
            string.IsNullOrEmpty(message.EventType) ||
            message.ArticleGuid == Guid.Empty ||
            message.PartitionGuids is null)
        {
            return null;
        }

        return message;
    }
}
