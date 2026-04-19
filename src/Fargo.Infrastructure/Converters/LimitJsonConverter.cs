using Fargo.Domain;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class LimitJsonConverter : JsonConverter<Limit>
{
    public override Limit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Must be an integer.");
        }

        var value = reader.GetInt32();

        try
        {
            return new Limit(value);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid Limit format.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Limit value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
