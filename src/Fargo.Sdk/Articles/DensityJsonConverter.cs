using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Serializes and deserializes <see cref="Density"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads density unit abbreviations (e.g. "kg/m³", "g/cm³", "lb/ft³").
/// Writes the value and unit abbreviation — no unit conversion on output.
/// </summary>
internal sealed class DensityJsonConverter : JsonConverter<Density>
{
    /// <inheritdoc />
    public override Density Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Density must be an object with 'value' (number) and 'unit' (string) fields.");
        }

        double value = 0;
        DensityUnit unit = DensityUnit.KilogramPerCubicMeter;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name inside density object.");
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
                        ?? throw new JsonException("Density 'unit' must be a string.");
                    try
                    {
                        unit = Density.ParseUnit(unitStr);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new JsonException($"Unknown density unit '{unitStr}'.", ex);
                    }
                    break;
            }
        }

        return new Density(value, unit);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Density value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", value.ToAbbreviation());
        writer.WriteEndObject();
    }
}
