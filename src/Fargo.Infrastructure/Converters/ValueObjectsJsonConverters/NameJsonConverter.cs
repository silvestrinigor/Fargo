using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters.ValueObjectsJsonConverters
{
    public class NameJsonConverter : JsonConverter<Name>
    {
        public override Name Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Name must be a string.");

            var value = reader.GetString()!;

            return new Name(value);
        }

        public override void Write(Utf8JsonWriter writer, Name value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
