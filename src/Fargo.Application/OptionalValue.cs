namespace Fargo.Application;

/// <summary>
/// Represents an optional value.
/// </summary>
/// <remarks>
/// Used to distinguish between an unspecified value
/// and an explicit null assignment.
/// </remarks>
/// <typeparam name="TValue">
/// Value type.
/// </typeparam>
/// <param name="IsSpecified">
/// Indicates whether the value was provided.
/// </param>
/// <param name="Value">
/// Optional value.
/// </param>
public readonly record struct OptionalValue<TValue>(bool IsSpecified, TValue? Value)
    where TValue : struct
{
    public static OptionalValue<TValue> FromValue(TValue? value) => new(true, value);
}
