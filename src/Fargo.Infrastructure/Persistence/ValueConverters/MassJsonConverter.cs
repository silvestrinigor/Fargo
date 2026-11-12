using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;
using System.Text.Json;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Persistence.ValueConverters;

/// <summary>
/// Stored as a <c>jsonb</c> object <c>{ "value": 2.5, "unit": "kg" }</c>. Value and unit are kept exactly as
/// given (no unit conversion) and stay individually queryable.
/// </summary>
/// <remarks>
/// The value is serialized with the round-trip <c>"R"</c> format and the invariant culture so it
/// deserializes back to the exact same <see cref="double"/>.
/// </remarks>
public sealed class MassJsonConverter()
    : ValueConverter<Mass, string>(
        mass => Serialize(mass),
        json => Deserialize(json))
{
    private static string Serialize(Mass mass)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("value", mass.Value);
            writer.WriteString("unit", Mass.GetAbbreviation(mass.Unit));
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static Mass Deserialize(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var value = root.GetProperty("value").GetDouble();
        var unitAbbreviation = root.GetProperty("unit").GetString()
            ?? throw new FormatException("Mass JSON is missing the 'unit' property.");

        var unit = UnitParser.Default.Parse<MassUnit>(unitAbbreviation);

        return Mass.From(value, unit);
    }
}
