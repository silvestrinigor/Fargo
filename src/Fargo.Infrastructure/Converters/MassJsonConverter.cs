using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Converters;

/// <summary>
/// Serializes and deserializes <see cref="Mass"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads any UnitsNet mass unit abbreviation (e.g. "g", "kg", "mg", "lb", "oz").
/// Always writes in grams for a canonical, unit-agnostic response.
/// </summary>
public sealed class MassJsonConverter : JsonConverter<Mass>
{
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
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name inside mass object.");

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
                        unit = UnitParser.Default.Parse<MassUnit>(unitStr);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonException($"Unknown mass unit '{unitStr}'.", ex);
                    }
                    break;
            }
        }

        return Mass.From(value, unit);
    }

    public override void Write(Utf8JsonWriter writer, Mass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Grams);
        writer.WriteString("unit", "g");
        writer.WriteEndObject();
    }
}
