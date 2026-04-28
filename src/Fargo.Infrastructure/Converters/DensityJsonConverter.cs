using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Converters;

/// <summary>
/// Serializes and deserializes <see cref="Density"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads any UnitsNet density unit abbreviation (e.g. "kg/m³", "g/cm³", "lb/ft³").
/// Writes the value and unit exactly as stored — no unit conversion on output.
/// </summary>
public sealed class DensityJsonConverter : JsonConverter<Density>
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
                        unit = UnitParser.Default.Parse<DensityUnit>(unitStr);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonException($"Unknown density unit '{unitStr}'.", ex);
                    }
                    break;
            }
        }

        return Density.From(value, unit);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Density value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", Density.GetAbbreviation(value.Unit));
        writer.WriteEndObject();
    }
}
