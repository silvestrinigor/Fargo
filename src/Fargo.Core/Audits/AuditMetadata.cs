namespace Fargo.Core.Audits;

public sealed class AuditMetadata
{
    private readonly Dictionary<string, AuditValue> values = [];

    public IReadOnlyDictionary<string, AuditValue> Values => values;

    public AuditMetadata() { }

    public void Add(string name, AuditValue value)
    {
        if (!values.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Property '{name}' already existis.");
        }
    }
}
