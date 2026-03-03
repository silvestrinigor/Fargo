using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters.ValueObjectsJsonConverters
{
    public class PasswordJsonConverter : JsonConverter<Password>
    {
        public override Password Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Password must be a string.");

            var value = reader.GetString()!;

            return new Password(value);
        }

        public override void Write(Utf8JsonWriter writer, Password value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}