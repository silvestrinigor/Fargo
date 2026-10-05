namespace Fargo.Core.Inbox;

/// <summary>
/// Represents a value stored in an inbox.
/// </summary>
public abstract record InboxValue
{
    /// <summary>
    /// Represents a string value.
    /// </summary>
    /// <param name="Value">The string value.</param>
    public sealed record String(string Value) : InboxValue;

    /// <summary>
    /// Represents a numeric value.
    /// </summary>
    /// <param name="Value">The numeric value.</param>
    public sealed record Number(decimal Value) : InboxValue;

    /// <summary>
    /// Represents a Boolean value.
    /// </summary>
    /// <param name="Value">The Boolean value.</param>
    public sealed record Boolean(bool Value) : InboxValue;

    /// <summary>
    /// Represents a null value.
    /// </summary>
    public sealed record Null : InboxValue;

    /// <summary>
    /// Represents an object containing named inbox values.
    /// </summary>
    /// <param name="Value">The properties and their corresponding values.</param>
    public sealed record Object(IReadOnlyDictionary<string, InboxValue> Value) : InboxValue;

    /// <summary>
    /// Represents an array of inbox values.
    /// </summary>
    /// <param name="Values">The values contained in the array.</param>
    public sealed record Array(IReadOnlyCollection<InboxValue> Values) : InboxValue;
}
