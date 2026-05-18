namespace Fargo.Application.Workspaces;

public enum ReservedGuidKind
{
    Article = 0,
    Item = 1,
    Partition = 2
}

public readonly record struct ReservedArticleGuid(Guid Value);

public readonly record struct ReservedItemGuid(Guid Value);

public readonly record struct ReservedPartitionGuid(Guid Value);

public interface IReservedGuidSession
{
    ReservedArticleGuid ReserveArticleGuid();

    ReservedItemGuid ReserveItemGuid();

    ReservedPartitionGuid ReservePartitionGuid();

    ReservedArticleGuid RegisterArticleGuid(Guid guid);

    ReservedItemGuid RegisterItemGuid(Guid guid);

    ReservedPartitionGuid RegisterPartitionGuid(Guid guid);

    Guid ResolveArticleGuid(ReservedArticleGuid? guid);

    Guid ResolveItemGuid(ReservedItemGuid? guid);

    Guid ResolvePartitionGuid(ReservedPartitionGuid? guid);

    void ValidateArticleGuid(Guid guid);

    void ValidateItemGuid(Guid guid);

    void ValidatePartitionGuid(Guid guid);
}

public sealed class ReservedGuidSession : IReservedGuidSession
{
    private readonly HashSet<Guid> articleGuids = [];
    private readonly HashSet<Guid> itemGuids = [];
    private readonly HashSet<Guid> partitionGuids = [];

    public ReservedArticleGuid ReserveArticleGuid()
        => new(Reserve(articleGuids));

    public ReservedItemGuid ReserveItemGuid()
        => new(Reserve(itemGuids));

    public ReservedPartitionGuid ReservePartitionGuid()
        => new(Reserve(partitionGuids));

    public ReservedArticleGuid RegisterArticleGuid(Guid guid)
    {
        Register(articleGuids, guid);
        return new ReservedArticleGuid(guid);
    }

    public ReservedItemGuid RegisterItemGuid(Guid guid)
    {
        Register(itemGuids, guid);
        return new ReservedItemGuid(guid);
    }

    public ReservedPartitionGuid RegisterPartitionGuid(Guid guid)
    {
        Register(partitionGuids, guid);
        return new ReservedPartitionGuid(guid);
    }

    public Guid ResolveArticleGuid(ReservedArticleGuid? guid)
    {
        if (guid is not { } value)
        {
            return ReserveArticleGuid().Value;
        }

        ValidateArticleGuid(value.Value);

        return value.Value;
    }

    public Guid ResolveItemGuid(ReservedItemGuid? guid)
    {
        if (guid is not { } value)
        {
            return ReserveItemGuid().Value;
        }

        ValidateItemGuid(value.Value);

        return value.Value;
    }

    public Guid ResolvePartitionGuid(ReservedPartitionGuid? guid)
    {
        if (guid is not { } value)
        {
            return ReservePartitionGuid().Value;
        }

        ValidatePartitionGuid(value.Value);

        return value.Value;
    }

    public void ValidateArticleGuid(Guid guid)
        => Validate(articleGuids, guid, ReservedGuidKind.Article);

    public void ValidateItemGuid(Guid guid)
        => Validate(itemGuids, guid, ReservedGuidKind.Item);

    public void ValidatePartitionGuid(Guid guid)
        => Validate(partitionGuids, guid, ReservedGuidKind.Partition);

    private static Guid Reserve(HashSet<Guid> reservedGuids)
    {
        var guid = Guid.NewGuid();
        reservedGuids.Add(guid);

        return guid;
    }

    private static void Register(HashSet<Guid> reservedGuids, Guid guid)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException("Reserved GUID cannot be empty.", nameof(guid));
        }

        reservedGuids.Add(guid);
    }

    private static void Validate(
        HashSet<Guid> reservedGuids,
        Guid guid,
        ReservedGuidKind kind)
    {
        if (!reservedGuids.Contains(guid))
        {
            throw new UnreservedGuidFargoApplicationException(kind, guid);
        }
    }
}

public sealed class UnreservedGuidFargoApplicationException(
    ReservedGuidKind kind,
    Guid guid
) : FargoApplicationException($"{kind} GUID '{guid}' was not reserved in the current application session.")
{
    public ReservedGuidKind Kind { get; } = kind;

    public Guid Guid { get; } = guid;
}
