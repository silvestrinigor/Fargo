using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class TokenJsonConverter : JsonConverter<Token>
{
    public override Token Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Token must be a string.");
        }

        var value = reader.GetString()!;

        try
        {
            return new Token(value);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid Token format.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Token value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
