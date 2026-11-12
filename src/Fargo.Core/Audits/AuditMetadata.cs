namespace Fargo.Core.Audits;

/// <summary>
/// A tree of named <see cref="AuditValue"/>s persisted as jsonb. Populated by the audit pipeline;
/// the domain does not interpret its contents.
/// </summary>
public sealed class AuditMetadata
{
    public IReadOnlyDictionary<string, AuditValue> Values => values;

    private readonly Dictionary<string, AuditValue> values = [];

    public AuditMetadata() { }

    /// <summary>
    /// Adds a value; throws <see cref="InvalidOperationException"/> if the name is already present.
    /// </summary>
    public void Add(string name, AuditValue value)
    {
        if (!values.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Property '{name}' already exists.");
        }
    }
}
