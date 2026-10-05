namespace Fargo.Core.Audits;

/// <summary>
/// Represents a value stored in an audit record.
/// </summary>
public abstract record AuditValue
{
    /// <summary>
    /// Represents a string value.
    /// </summary>
    /// <param name="Value">The string value.</param>
    public sealed record String(string Value) : AuditValue;

    /// <summary>
    /// Represents a numeric value.
    /// </summary>
    /// <param name="Value">The numeric value.</param>
    public sealed record Number(decimal Value) : AuditValue;

    /// <summary>
    /// Represents a Boolean value.
    /// </summary>
    /// <param name="Value">The Boolean value.</param>
    public sealed record Boolean(bool Value) : AuditValue;

    /// <summary>
    /// Represents a null value.
    /// </summary>
    public sealed record Null : AuditValue;

    /// <summary>
    /// Represents an object containing named audit values.
    /// </summary>
    /// <param name="Value">The properties and their corresponding values.</param>
    public sealed record Object(IReadOnlyDictionary<string, AuditValue> Value) : AuditValue;

    /// <summary>
    /// Represents an array of audit values.
    /// </summary>
    /// <param name="Values">The values contained in the array.</param>
    public sealed record Array(IReadOnlyCollection<AuditValue> Values) : AuditValue;
}
