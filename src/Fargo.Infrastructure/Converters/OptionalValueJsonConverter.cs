using Fargo.Application;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class OptionalValueJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(OptionalValue<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalValueJsonConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class OptionalValueJsonConverter<TValue> : JsonConverter<OptionalValue<TValue>>
    where TValue : struct
{
    public override OptionalValue<TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return OptionalValue<TValue>.FromValue(null);
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return OptionalValue<TValue>.FromValue(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        OptionalValue<TValue> value,
        JsonSerializerOptions options)
    {
        if (!value.IsSpecified || value.Value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value.Value, options);
    }
}
