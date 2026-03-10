using Fargo.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.HttpApi.Converters
{
    public sealed class FirstNameJsonConverter : JsonConverter<FirstName>
    {
        public override FirstName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Must be an string.");

            var value = reader.GetString()!;

            try
            {
                return new FirstName(value);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException("Invalid FirstName format.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, FirstName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}