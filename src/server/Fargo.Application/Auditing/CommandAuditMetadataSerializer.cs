using Fargo.Core.Audits;
using System.Collections;
using System.Reflection;

namespace Fargo.Application.Auditing;

/// <summary>
/// Serializes a command object's data into an <see cref="AuditMetadata"/> tree
/// for audit logging.
/// </summary>
/// <remarks>
/// The serializer reflects over the command's public readable properties and
/// maps each to an <see cref="AuditValue"/>. Sensitive values (passwords,
/// secrets, tokens and hashes, matched by property name) are redacted so secrets
/// never reach the audit store.
/// </remarks>
public static class CommandAuditMetadataSerializer
{
    private const string redactedPlaceholder = "[REDACTED]";

    // Property-name fragments that mark a value as sensitive. Matched
    // case-insensitively as substrings so e.g. "Password", "NewPassword",
    // "ClientSecret", "RefreshToken", "SecretHash" are all redacted.
    private static readonly string[] sensitiveNameFragments =
    [
        "password",
        "secret",
        "token",
        "hash",
    ];

    // Guards against runaway/cyclic graphs. Command/DTO graphs are shallow, so a
    // small bound is safe and avoids pathological recursion.
    private const int maxDepth = 8;

    /// <summary>
    /// Builds an <see cref="AuditMetadata"/> snapshot from the specified command.
    /// </summary>
    /// <param name="command">The command whose data is captured.</param>
    /// <returns>The metadata describing the command's data.</returns>
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

        // Stop descending at the depth bound: render as string rather than recurse.
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

            // A value object with no readable members (or only its own string
            // form) is most useful rendered as its string representation.
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
