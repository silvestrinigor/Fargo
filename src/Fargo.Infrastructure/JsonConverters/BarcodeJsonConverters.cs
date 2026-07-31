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

public sealed class Ean8JsonConverter : JsonConverter<Ean8>
{
    public override Ean8 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Ean8)));

    public override void Write(Utf8JsonWriter writer, Ean8 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class UpcAJsonConverter : JsonConverter<UpcA>
{
    public override UpcA Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(UpcA)));

    public override void Write(Utf8JsonWriter writer, UpcA value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class UpcEJsonConverter : JsonConverter<UpcE>
{
    public override UpcE Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(UpcE)));

    public override void Write(Utf8JsonWriter writer, UpcE value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class Code128JsonConverter : JsonConverter<Code128>
{
    public override Code128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Code128)));

    public override void Write(Utf8JsonWriter writer, Code128 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class Code39JsonConverter : JsonConverter<Code39>
{
    public override Code39 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Code39)));

    public override void Write(Utf8JsonWriter writer, Code39 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class Itf14JsonConverter : JsonConverter<Itf14>
{
    public override Itf14 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Itf14)));

    public override void Write(Utf8JsonWriter writer, Itf14 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class Gs1128JsonConverter : JsonConverter<Gs1128>
{
    public override Gs1128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(Gs1128)));

    public override void Write(Utf8JsonWriter writer, Gs1128 value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class QrCodeJsonConverter : JsonConverter<QrCode>
{
    public override QrCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(QrCode)));

    public override void Write(Utf8JsonWriter writer, QrCode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

public sealed class DataMatrixJsonConverter : JsonConverter<DataMatrix>
{
    public override DataMatrix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(BarcodeJsonConverter.ReadString(ref reader, nameof(DataMatrix)));

    public override void Write(Utf8JsonWriter writer, DataMatrix value, JsonSerializerOptions options)
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
