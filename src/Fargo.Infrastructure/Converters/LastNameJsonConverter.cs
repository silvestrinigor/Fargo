using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class LastNameJsonConverter : JsonConverter<LastName>
{
    public override LastName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Must be an string.");
        }

        var value = reader.GetString()!;

        try
        {
            return new LastName(value);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid LastName format.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, LastName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
