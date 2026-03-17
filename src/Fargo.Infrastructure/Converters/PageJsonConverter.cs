using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class PageJsonConverter : JsonConverter<Page>
{
    public override Page Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Must be an integer.");
        }

        var value = reader.GetInt32();

        try
        {
            return new Page(value);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid Page format.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Page value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
