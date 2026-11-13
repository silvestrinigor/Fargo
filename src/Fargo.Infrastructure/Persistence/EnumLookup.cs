namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// A row of a reference table that mirrors an enum, so the database can say what a stored number means and
/// reject numbers that are not members of the enum.
/// </summary>
/// <remarks>
/// The code keeps using the enum and never reads these tables. Their rows come from the enum itself, in the
/// migrations, so the enum stays the only source of truth. Enum members are append-only: renumbering or
/// removing one fails while rows still use it.
/// </remarks>
public sealed class EnumLookup<TEnum>
    where TEnum : struct, Enum
{
    public TEnum Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
