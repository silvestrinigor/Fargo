using System.Text.Json;

namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// Serializes integration event messages to the JSON format published to the
/// broker (camelCase property names, no indentation).
/// </summary>
public static class IntegrationEventJson
{
    /// <summary>
    /// The serializer options used for every integration event message.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes an article event message.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The JSON representation of the message.</returns>
    public static string Serialize(ArticleEventMessage message)
        => JsonSerializer.Serialize(message, Options);

    /// <summary>
    /// Deserializes an article event message published with the current
    /// <see cref="ArticleEventMessage.SchemaVersion"/>.
    /// </summary>
    /// <param name="body">The UTF-8 JSON body of the message.</param>
    /// <returns>
    /// The message, or <see langword="null"/> when the body is not a complete
    /// article event (malformed JSON, or a required field such as
    /// <see cref="ArticleEventMessage.PartitionGuids"/> is missing).
    /// </returns>
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
