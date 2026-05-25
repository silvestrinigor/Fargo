namespace Fargo.Core.Articles;

public enum ArticleType
{
    Default = 1,
    Variation = 2,
    Pack = 3,
    Kit = 4,
    Container = 5
}

[Flags]
public enum ArticleModifiedType
{
    None = 0,
    General = 1 << 0,
    MetricsChanged = 1 << 1,
    BarcodesChanged = 1 << 2,
    PartitionsChanged = 1 << 3,
    Container = 1 << 4,
    Relation = 1 << 5,
    Activated = 1 << 6,
    Deactivated = 1 << 7
}
