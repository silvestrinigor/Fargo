namespace Fargo.Core.Audits;

/// <summary>
/// Represents a snapshot of a command's data stored on an audit record.
/// </summary>
/// <remarks>
/// Metadata is a tree of named <see cref="AuditValue"/>s persisted as jsonb. It
/// is populated by the audit pipeline from the executed command's data; the
/// domain does not interpret its contents.
/// </remarks>
public sealed class AuditMetadata
{
    /// <summary>
    /// Gets the metadata values.
    /// </summary>
    public IReadOnlyDictionary<string, AuditValue> Values => values;

    private readonly Dictionary<string, AuditValue> values = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditMetadata"/> class.
    /// </summary>
    public AuditMetadata() { }

    /// <summary>
    /// Adds a metadata value.
    /// </summary>
    /// <param name="name">The name of the metadata property.</param>
    /// <param name="value">The value of the metadata property.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a metadata property with the specified name already exists.
    /// </exception>
    public void Add(string name, AuditValue value)
    {
        if (!values.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Property '{name}' already exists.");
        }
    }
}
