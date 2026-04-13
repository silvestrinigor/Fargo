using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;
using DomainMass = Fargo.Domain.ValueObjects.Mass;

namespace Fargo.Infrastructure.Converters;

/// <summary>
/// Serializes and deserializes <see cref="DomainMass"/> as <c>{ "value": number, "unit": string }</c>.
/// Reads any UnitsNet mass unit abbreviation (e.g. "g", "kg", "mg", "lb", "oz").
/// Writes the value and unit exactly as stored in the domain object — no unit conversion on output.
/// </summary>
public sealed class MassJsonConverter : JsonConverter<DomainMass>
{
    public override DomainMass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        return new DomainMass(value, unit);
    }

    public override void Write(Utf8JsonWriter writer, DomainMass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", UnitsNet.Mass.GetAbbreviation(value.Unit));
        writer.WriteEndObject();
    }
}
