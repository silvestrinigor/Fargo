using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Color must be a string.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Color cannot be empty.");
        }

        return ColorTranslator.FromHtml(value);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        => writer.WriteStringValue(ColorTranslator.ToHtml(value));
}
