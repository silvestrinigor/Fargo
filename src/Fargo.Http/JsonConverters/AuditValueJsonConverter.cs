using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Core.Audits;

/// <summary>
/// Converts <see cref="AuditValue"/> instances to and from JSON values.
/// </summary>
public sealed class AuditValueJsonConverter : JsonConverter<AuditValue>
{
    /// <inheritdoc />
    public override AuditValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String =>
                new AuditValue.String(reader.GetString()!),

            JsonTokenType.Number =>
                new AuditValue.Number(reader.GetDecimal()),

            JsonTokenType.True =>
                new AuditValue.Boolean(true),

            JsonTokenType.False =>
                new AuditValue.Boolean(false),

            JsonTokenType.Null =>
                new AuditValue.Null(),

            JsonTokenType.StartObject =>
                ReadObject(ref reader),

            JsonTokenType.StartArray =>
                ReadArray(ref reader),

            _ => throw new JsonException(
                $"Unexpected token '{reader.TokenType}' when reading {nameof(AuditValue)}.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        AuditValue value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case AuditValue.String stringValue:
                writer.WriteStringValue(stringValue.Value);
                break;

            case AuditValue.Number numberValue:
                writer.WriteNumberValue(numberValue.Value);
                break;

            case AuditValue.Boolean booleanValue:
                writer.WriteBooleanValue(booleanValue.Value);
                break;

            case AuditValue.Null:
                writer.WriteNullValue();
                break;

            case AuditValue.Object objectValue:
                writer.WriteStartObject();

                foreach (var property in objectValue.Value)
                {
                    writer.WritePropertyName(property.Key);
                    JsonSerializer.Serialize(
                        writer,
                        property.Value,
                        options);
                }

                writer.WriteEndObject();
                break;

            case AuditValue.Array arrayValue:
                writer.WriteStartArray();

                foreach (var item in arrayValue.Values)
                {
                    JsonSerializer.Serialize(
                        writer,
                        item,
                        options);
                }

                writer.WriteEndArray();
                break;

            default:
                throw new JsonException(
                    $"Unsupported {nameof(AuditValue)} type '{value.GetType()}'.");
        }
    }

    private static AuditValue.Object ReadObject(
        ref Utf8JsonReader reader)
    {
        var values = new Dictionary<string, AuditValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new AuditValue.Object(values);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException(
                    "Expected a property name when reading an audit object.");
            }

            var propertyName = reader.GetString()!;

            if (!reader.Read())
            {
                throw new JsonException(
                    "Unexpected end of JSON while reading an audit object.");
            }

            values[propertyName] = ReadValue(ref reader);
        }

        throw new JsonException(
            "Unexpected end of JSON while reading an audit object.");
    }

    private static AuditValue.Array ReadArray(
        ref Utf8JsonReader reader)
    {
        var values = new List<AuditValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return new AuditValue.Array(values);
            }

            values.Add(ReadValue(ref reader));
        }

        throw new JsonException(
            "Unexpected end of JSON while reading an audit array.");
    }

    private static AuditValue ReadValue(
        ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String =>
                new AuditValue.String(reader.GetString()!),

            JsonTokenType.Number =>
                new AuditValue.Number(reader.GetDecimal()),

            JsonTokenType.True =>
                new AuditValue.Boolean(true),

            JsonTokenType.False =>
                new AuditValue.Boolean(false),

            JsonTokenType.Null =>
                new AuditValue.Null(),

            JsonTokenType.StartObject =>
                ReadObject(ref reader),

            JsonTokenType.StartArray =>
                ReadArray(ref reader),

            _ => throw new JsonException(
                $"Unexpected token '{reader.TokenType}' when reading {nameof(AuditValue)}.")
        };
    }
}
