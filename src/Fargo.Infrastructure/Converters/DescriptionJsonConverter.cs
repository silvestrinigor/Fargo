using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters
{
    public class DescriptionJsonConverter : JsonConverter<Description>
    {
        public override Description Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Description must be a string.");

            var value = reader.GetString()!;

            return new Description(value);
        }

        public override void Write(Utf8JsonWriter writer, Description value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
