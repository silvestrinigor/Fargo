using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.HttpApi.Converters
{
    public class TokenJsonConverter : JsonConverter<Token>
    {
        public override Token Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Must be a string.");

            var value = reader.GetString()!;

            return new Token(value);
        }

        public override void Write(Utf8JsonWriter writer, Token value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}