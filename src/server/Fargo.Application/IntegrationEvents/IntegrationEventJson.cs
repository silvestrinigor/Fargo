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
}
