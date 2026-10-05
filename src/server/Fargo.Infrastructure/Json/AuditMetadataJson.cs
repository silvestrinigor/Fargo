using Fargo.Core.Audits;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fargo.Infrastructure.Json;

internal static class AuditMetadataJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false
    };

    public static string Serialize(AuditMetadata metadata)
    {
        var json = new JsonObject();

        foreach (var (name, value) in metadata.Values)
        {
            json[name] = ToJsonNode(value);
        }

        return json.ToJsonString(Options);
    }

    public static AuditMetadata Deserialize(string json)
    {
        var node = JsonNode.Parse(json)
            ?? throw new JsonException("Audit properties JSON is null.");

        if (node is not JsonObject jsonObject)
        {
            throw new JsonException("Audit metadata JSON must contain an object.");
        }

        var properties = new AuditMetadata();

        foreach (var m in jsonObject)
        {
            properties.Add(m.Key, FromJsonNode(m.Value));
        }

        return properties;
    }

    private static JsonNode? ToJsonNode(AuditValue value)
    {
        return value switch
        {
            AuditValue.String x => JsonValue.Create(x.Value),
            AuditValue.Number x => JsonValue.Create(x.Value),
            AuditValue.Boolean x => JsonValue.Create(x.Value),
            AuditValue.Null x => null,
            AuditValue.Object x => ToJsonObject(x),
            AuditValue.Array x => ToJsonArray(x),
            _ => throw new JsonException($"Unsupported audit value type: {value.GetType().Name}")
        };
    }

    private static JsonObject? ToJsonObject(AuditValue.Object value)
    {
        var json = new JsonObject();

        foreach (var (name, property) in value.Value)
        {
            json[name] = ToJsonNode(property);
        }

        return json;
    }

    private static JsonArray ToJsonArray(AuditValue.Array value)
    {
        var json = new JsonArray();

        foreach (var item in value.Values)
        {
            json.Add(ToJsonNode(item));
        }

        return json;
    }

    private static AuditValue FromJsonNode(JsonNode? node)
    {
        if (node is null)
        {
            return new AuditValue.Null();
        }

        return node switch
        {
            JsonObject obj => FromJsonObject(obj),
            JsonArray array => FromJsonArray(array),
            JsonValue value => FromJsonValue(value),
            _ => throw new JsonException($"Unsupported JSON node: {node.GetType().Name}")
        };
    }

    private static AuditValue.Object FromJsonObject(JsonObject json)
    {
        var properties = new Dictionary<string, AuditValue>();

        foreach (var property in json)
        {
            properties.Add(property.Key, FromJsonNode(property.Value));
        }

        return new AuditValue.Object(properties);
    }

    private static AuditValue.Array FromJsonArray(JsonArray json)
    {
        var values = json.Select(FromJsonNode).ToList();

        return new AuditValue.Array(values);
    }

    private static AuditValue FromJsonValue(JsonValue value)
    {
        if (value.TryGetValue<string>(out var stringValue))
        {
            return new AuditValue.String(stringValue);
        }
        if (value.TryGetValue<bool>(out var boolValue))
        {
            return new AuditValue.Boolean(boolValue);
        }
        if (value.TryGetValue<decimal>(out var numberValue))
        {
            return new AuditValue.Number(numberValue);
        }

        throw new JsonException($"Unsupported JSON value: {value}");
    }
}
