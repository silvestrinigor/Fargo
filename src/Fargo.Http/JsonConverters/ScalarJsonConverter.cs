using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Http.Shared.JsonConverters;

public sealed class ScalarJsonConverter : JsonConverter<Scalar>
{
    public override Scalar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return Scalar.From(reader.GetDouble(), ScalarUnit.Amount);

            case JsonTokenType.String:
                {
                    var text = reader.GetString()!;

                    // Try parsing as just a number first.
                    if (double.TryParse(
                            text,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out var value))
                    {
                        return Scalar.From(value, ScalarUnit.Amount);
                    }

                    // Otherwise let UnitsNet parse "<value> <unit>".
                    try
                    {
                        return Scalar.Parse(text);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonException($"Invalid scalar '{text}'.", ex);
                    }
                }

            default:
                throw new JsonException("Scalar must be a number or string.");
        }
    }

    public override void Write(Utf8JsonWriter writer, Scalar value, JsonSerializerOptions options)
    {
        if (value.Unit == ScalarUnit.Amount)
        {
            // Write plain number for the default unit.
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteStringValue($"{value.Value} {Scalar.GetAbbreviation(value.Unit)}");
        }
    }
}
