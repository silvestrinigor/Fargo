namespace Fargo.Core.Audits;

public abstract record AuditValue
{
    public sealed record String(string Value) : AuditValue;

    public sealed record Number(decimal Value) : AuditValue;

    public sealed record Boolean(bool Value) : AuditValue;

    public sealed record Null : AuditValue;

    public sealed record Object(IReadOnlyDictionary<string, AuditValue> Value) : AuditValue;

    public sealed record Array(IReadOnlyCollection<AuditValue> Values) : AuditValue;
}
