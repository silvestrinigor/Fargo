using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.HttpApi.Converters
{
    public class NameidJsonConverter : JsonConverter<Nameid>
    {
        public override Nameid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Nameid must be a string.");

            var value = reader.GetString()!;

            try
            {
                return new Nameid(value);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException("Invalid Nameid format.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, Nameid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}