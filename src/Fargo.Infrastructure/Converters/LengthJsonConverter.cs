using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Converters;

/// <summary>
/// Serializes and deserializes <see cref="Length"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads any UnitsNet length unit abbreviation (e.g. "mm", "cm", "m", "km", "in", "ft").
/// Writes the value and unit exactly as stored — no unit conversion on output.
/// </summary>
public sealed class LengthJsonConverter : JsonConverter<Length>
{
    public override Length Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Length must be an object with 'value' (number) and 'unit' (string) fields.");
        }

        double value = 0;
        LengthUnit unit = LengthUnit.Meter;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name inside length object.");
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
                        ?? throw new JsonException("Length 'unit' must be a string.");
                    try
                    {
                        unit = UnitParser.Default.Parse<LengthUnit>(unitStr);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonException($"Unknown length unit '{unitStr}'.", ex);
                    }
                    break;
            }
        }

        return Length.From(value, unit);
    }

    public override void Write(Utf8JsonWriter writer, Length value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", Length.GetAbbreviation(value.Unit));
        writer.WriteEndObject();
    }
}
