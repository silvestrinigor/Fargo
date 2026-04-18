using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Models.TreeModels;

/// <summary>
/// Represents a strongly-typed identifier for a tree node.
/// </summary>
/// <remarks>
/// The identifier is composed of:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="TreeNodeType"/> represented as its numeric value.
/// </description>
/// </item>
/// <item>
/// <description>
/// The <see cref="Guid"/> of the associated entity.
/// </description>
/// </item>
/// </list>
///
/// The string format is:
/// <c>{TreeNodeType}:{EntityGuid}</c>
///
/// Example:
/// <c>"1:3f5c2e3b-8c3a-4d6e-b9d1-2c8e5f6a7b8c"</c>
/// </remarks>
public readonly struct Nodeid : IParsable<Nodeid>
{
    /// <summary>
    /// Gets the serialized value of the node identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    public TreeNodeType TreeNodeType { get; }

    /// <summary>
    /// Gets the unique identifier of the associated entity.
    /// </summary>
    public Guid EntityGuid { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Nodeid"/> struct.
    /// </summary>
    /// <param name="treeNodeType">The type of the node.</param>
    /// <param name="entityGuid">The unique identifier of the entity.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="entityGuid"/> is empty.
    /// </exception>
    public Nodeid(TreeNodeType treeNodeType, Guid entityGuid)
    {
        if (entityGuid == Guid.Empty)
        {
            throw new ArgumentException("Entity GUID cannot be empty.", nameof(entityGuid));
        }

        TreeNodeType = treeNodeType;
        EntityGuid = entityGuid;

        Value = $"{(int)treeNodeType}:{entityGuid}";
    }

    /// <summary>
    /// Converts a <see cref="Nodeid"/> to its string representation.
    /// </summary>
    public static implicit operator string(Nodeid nodeid)
        => nodeid.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="Nodeid"/>.
    /// </summary>
    public static explicit operator Nodeid(string value)
        => Parse(value, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public static Nodeid Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid Nodeid format: '{s}'.");
        }

        return result;
    }

    /// <inheritdoc />
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

        var parts = s.Split(':', 2, StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
        {
            return false;
        }

        // Parse TreeNodeType as integer
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var typeValue))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(TreeNodeType), typeValue))
        {
            return false;
        }

        var type = (TreeNodeType)typeValue;

        // Parse Guid
        if (!Guid.TryParse(parts[1], out var guid) || guid == Guid.Empty)
        {
            return false;
        }

        result = new Nodeid(type, guid);
        return true;
    }
}
