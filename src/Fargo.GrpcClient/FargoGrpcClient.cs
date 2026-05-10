namespace Fargo.GrpcClient;

public sealed class FargoGrpcClient(
    FargoAuthenticationGrpcClient authentication,
    FargoArticleGrpcClient articles,
    FargoItemGrpcClient items,
    FargoPartitionGrpcClient partitions,
    FargoUserGrpcClient users,
    FargoUserGroupGrpcClient userGroups)
{
    public FargoAuthenticationGrpcClient Authentication { get; } = authentication;

    public FargoArticleGrpcClient Articles { get; } = articles;

    public FargoItemGrpcClient Items { get; } = items;

    public FargoPartitionGrpcClient Partitions { get; } = partitions;

    public FargoUserGrpcClient Users { get; } = users;

    public FargoUserGroupGrpcClient UserGroups { get; } = userGroups;
}
