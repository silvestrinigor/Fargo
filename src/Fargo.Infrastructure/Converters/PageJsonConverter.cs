using Fargo.Application.Commom;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters
{
    public class PageJsonConverter : JsonConverter<Page>
    {
        public override Page Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("Must be a integer.");

            var value = reader.GetInt32()!;

            return new Page(value);
        }

        public override void Write(Utf8JsonWriter writer, Page value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
