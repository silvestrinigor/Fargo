using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;
using System.Text.Json;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Persistence.ValueConverters;

/// <summary>
/// Stored as a <c>jsonb</c> object <c>{ "value": 10, "unit": "cm" }</c>. Value and unit are kept exactly as
/// given (no unit conversion) and stay individually queryable.
/// </summary>
public sealed class LengthJsonConverter()
    : ValueConverter<Length, string>(
        length => Serialize(length),
        json => Deserialize(json))
{
    private static string Serialize(Length length)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("value", length.Value);
            writer.WriteString("unit", Length.GetAbbreviation(length.Unit));
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static Length Deserialize(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var value = root.GetProperty("value").GetDouble();
        var unitAbbreviation = root.GetProperty("unit").GetString()
            ?? throw new FormatException("Length JSON is missing the 'unit' property.");

        var unit = UnitParser.Default.Parse<LengthUnit>(unitAbbreviation);

        return Length.From(value, unit);
    }
}
