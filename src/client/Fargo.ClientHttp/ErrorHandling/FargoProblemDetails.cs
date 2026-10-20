using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.ClientHttp.ErrorHandling;

/// <summary>
/// A minimal RFC 7807 problem details representation matching the shape the
/// Fargo server returns for error responses. Extension members (such as
/// <c>traceId</c>, <c>errorType</c>, <c>actorType</c> or <c>actionType</c>) are
/// captured in <see cref="Extensions"/>.
/// </summary>
public sealed class FargoProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extensions { get; set; }

    /// <summary>
    /// Reads an extension value as a string, returning <c>null</c> when it is
    /// absent or not a scalar that can be represented as text.
    /// </summary>
    public string? GetExtensionString(string name)
    {
        if (Extensions is null || !Extensions.TryGetValue(name, out var element))
        {
            return null;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }
}
