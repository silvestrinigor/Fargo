using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Api.Articles;

/// <summary>
/// Serializes and deserializes <see cref="Mass"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads mass unit abbreviations (e.g. "g", "kg", "mg", "lb", "oz").
/// Writes the value and unit abbreviation — no unit conversion on output.
/// </summary>
internal sealed class MassJsonConverter : JsonConverter<Mass>
{
    /// <inheritdoc />
    public override Mass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Mass must be an object with 'value' (number) and 'unit' (string) fields.");
        }

        double value = 0;
        MassUnit unit = MassUnit.Gram;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name inside mass object.");
            }

            string propName = reader.GetString()!;
            reader.Read();

            switch (propName.ToLowerInvariant())
            {
                case "value":
                    value = reader.GetDouble();
                    break;
                case "unit":
                    string unitStr = reader.GetString()
                        ?? throw new JsonException("Mass 'unit' must be a string.");
                    try
                    {
                        unit = Mass.ParseUnit(unitStr);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new JsonException($"Unknown mass unit '{unitStr}'.", ex);
                    }
                    break;
            }
        }

        return new Mass(value, unit);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Mass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", value.ToAbbreviation());
        writer.WriteEndObject();
    }
}
