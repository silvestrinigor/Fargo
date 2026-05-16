using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Sdk.Contracts;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly record struct Optional<TValue>(bool IsSpecified, TValue? Value)
{
    public static Optional<TValue> FromValue(TValue? value) => new(true, value);
}

public sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType &&
           typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class OptionalJsonConverter<TValue> : JsonConverter<Optional<TValue>>
{
    public override Optional<TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Optional<TValue>.FromValue(default);
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return Optional<TValue>.FromValue(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Optional<TValue> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
