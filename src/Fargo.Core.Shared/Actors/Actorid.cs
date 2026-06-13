namespace Fargo.Core.Shared.Actors;

public readonly struct ActorId :
    IEquatable<ActorId>,
    IParsable<ActorId>,
    ISpanParsable<ActorId>
{
    public Guid Guid { get; }

    public ActorType ActorType { get; }

    public ActorId(Guid guid, ActorType type)
    {
        Guid = guid;
        ActorType = type;
    }

    private const string actorDistriminator = "actor";

    public override string ToString()
        => $"{actorDistriminator}:{Guid}:{ActorType}";

    public static ActorId Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException("Invalid actor id.");
        }

        return result;
    }

    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        out ActorId result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var parts = s.Split(':');

        if (parts.Length != 3)
        {
            return false;
        }

        if (!string.Equals(parts[0], actorDistriminator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Guid.TryParse(parts[1], out var id))
        {
            return false;
        }

        if (!Enum.TryParse<ActorType>(
            parts[2],
            true,
            out var type))
        {
            return false;
        }

        result = new ActorId(id, type);
        return true;
    }

    public static ActorId Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => Parse(s.ToString(), provider);

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out ActorId result)
        => TryParse(s.ToString(), provider, out result);

    public bool Equals(ActorId other)
        => Guid == other.Guid && ActorType == other.ActorType;

    public override bool Equals(object? obj)
        => obj is ActorId other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Guid, ActorType);

    public static bool operator ==(ActorId left, ActorId right)
        => left.Equals(right);

    public static bool operator !=(ActorId left, ActorId right)
        => !left.Equals(right);
}
