using Fargo.Core.Shared.Barcodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class Ean13JsonConverter : JsonConverter<Ean13>
{
    public override Ean13 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Ean13)));

    public override void Write(Utf8JsonWriter writer, Ean13 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

file static class BarcodeJsonConverter
{
    public static string ReadString(ref Utf8JsonReader reader, string typeName)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"{typeName} must be a string.");
        }

        return reader.GetString() ?? throw new JsonException($"{typeName} cannot be null.");
    }
}
