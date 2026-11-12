using Fargo.Core.Audits;
using System.Collections;
using System.Reflection;

namespace Fargo.Application.Auditing;

/// <remarks>
/// Values whose property name contains password, secret, token or hash are redacted so secrets
/// never reach the audit store.
/// </remarks>
public static class CommandAuditMetadataSerializer
{
    private const string redactedPlaceholder = "[REDACTED]";

    // Matched case-insensitively as substrings, so "NewPassword" and "RefreshToken" are redacted.
    private static readonly string[] sensitiveNameFragments =
    [
        "password",
        "secret",
        "token",
        "hash",
    ];

    // Guards against cyclic graphs; command graphs are shallow.
    private const int maxDepth = 8;

    public static AuditMetadata Serialize(object command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var metadata = new AuditMetadata();

        foreach (var (name, value) in ReadProperties(command))
        {
            metadata.Add(name, ToAuditValue(name, value, depth: 1));
        }

        return metadata;
    }

    private static IEnumerable<(string Name, object? Value)> ReadProperties(object instance)
    {
        var properties = instance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            object? value;

            try
            {
                value = property.GetValue(instance);
            }
            catch
            {
                // A throwing getter must never break auditing; record its absence.
                value = null;
            }

            yield return (ToCamelCase(property.Name), value);
        }
    }

    private static AuditValue ToAuditValue(string name, object? value, int depth)
    {
        if (value is null)
        {
            return new AuditValue.Null();
        }

        if (IsSensitive(name))
        {
            return new AuditValue.String(redactedPlaceholder);
        }

        switch (value)
        {
            case bool b:
                return new AuditValue.Boolean(b);

            case string s:
                return new AuditValue.String(s);

            case Guid g:
                return new AuditValue.String(g.ToString());

            case Enum e:
                return new AuditValue.Number(Convert.ToDecimal(Convert.ToInt64(e)));

            case DateTimeOffset dto:
                return new AuditValue.String(dto.ToString("O"));

            case DateTime dt:
                return new AuditValue.String(dt.ToString("O"));

            case TimeSpan ts:
                return new AuditValue.Number(ts.Ticks);
        }

        var type = value.GetType();

        if (IsNumeric(type))
        {
            return new AuditValue.Number(Convert.ToDecimal(value));
        }

        if (depth >= maxDepth)
        {
            return new AuditValue.String(value.ToString() ?? string.Empty);
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<AuditValue>();

            foreach (var item in enumerable)
            {
                items.Add(ToAuditValue(name, item, depth + 1));
            }

            return new AuditValue.Array(items);
        }

        if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
        {
            var members = new Dictionary<string, AuditValue>();

            foreach (var (childName, childValue) in ReadProperties(value))
            {
                members[childName] = ToAuditValue(childName, childValue, depth + 1);
            }

            // A value object with no readable members is best rendered as its string form.
            if (members.Count == 0)
            {
                return new AuditValue.String(value.ToString() ?? string.Empty);
            }

            return new AuditValue.Object(members);
        }

        return new AuditValue.String(value.ToString() ?? string.Empty);
    }

    private static bool IsSensitive(string name)
    {
        foreach (var fragment in sensitiveNameFragments)
        {
            if (name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNumeric(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
