using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Models.TreeModels;

public readonly struct Nodeid : IParsable<Nodeid>
{
    public string Value { get; }

    public TreeNodeType TreeNodeType { get; }

    public Guid EntityGuid { get; }

    public Nodeid(TreeNodeType treeNodeType, Guid entityGuid)
    {
        TreeNodeType = treeNodeType;
        EntityGuid = entityGuid;
        Value = $"{treeNodeType.ToString().ToLowerInvariant()}:{entityGuid}";
    }

    public static implicit operator string(Nodeid nodeid)
        => nodeid.Value;

    public static explicit operator Nodeid(string value)
        => Parse(value, CultureInfo.InvariantCulture);

    public override string ToString()
        => Value;

    public static Nodeid Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid Nodeid format: '{s}'.");
        }

        return result;
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Nodeid result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var parts = s.Split(':', 2);

        if (parts.Length != 2)
        {
            return false;
        }

        if (!Enum.TryParse<TreeNodeType>(parts[0], true, out var type))
        {
            return false;
        }

        if (!Guid.TryParse(parts[1], out var guid))
        {
            return false;
        }

        result = new Nodeid(type, guid);

        return true;
    }
}
