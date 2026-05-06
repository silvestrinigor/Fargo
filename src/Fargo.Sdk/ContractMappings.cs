using Fargo.Sdk.Articles;
using Fargo.Sdk.Contracts;
using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Permissions;
using Fargo.Sdk.Contracts.UserGroups;
using Fargo.Sdk.Contracts.Users;

namespace Fargo.Sdk;

internal static class ContractMappings
{
    public static ArticleCreateRequest ToArticleCreateRequest(
        string name,
        string? description,
        IReadOnlyCollection<Guid>? partitions,
        ArticleBarcodes? barcodes,
        ArticleMetrics? metrics,
        TimeSpan? shelfLife,
        bool? isActive)
        => new(name, description, metrics.ToContract(), shelfLife, partitions, barcodes?.ToContract(), isActive);

    public static ArticleUpdateRequest ToArticleUpdateRequest(
        string name,
        string? description,
        IReadOnlyCollection<Guid>? partitions,
        ArticleBarcodes? barcodes,
        ArticleMetrics? metrics,
        TimeSpan? shelfLife,
        bool isActive)
        => new(name, description, metrics.ToContract(), shelfLife, partitions, barcodes?.ToContract(), isActive);

    public static ItemCreateRequest ToItemCreateRequest(Guid articleGuid, Guid? firstPartition, DateTimeOffset? productionDate)
        => new(articleGuid, productionDate, firstPartition is null ? null : [firstPartition.Value]);

    public static ItemUpdateRequest ToItemUpdateRequest(
        IReadOnlyCollection<Guid> partitions,
        Guid? parentContainerGuid)
        => new(partitions, parentContainerGuid);

    public static PartitionCreateRequest ToPartitionCreateRequest(
        string name,
        string? description,
        Guid? parentPartitionGuid)
        => new(name, description, parentPartitionGuid);

    public static PartitionUpdateRequest ToPartitionUpdateRequest(
        string? name,
        string? description,
        Guid? parentPartitionGuid,
        bool? isActive)
        => new(name, description, parentPartitionGuid, isActive);

    public static UserCreateRequest ToUserCreateRequest(
        string nameid,
        string password,
        string? firstName,
        string? lastName,
        string? description,
        IReadOnlyCollection<ActionType>? permissions,
        TimeSpan? defaultPasswordExpirationPeriod,
        Guid? firstPartition)
        => new(
            nameid,
            password,
            firstName,
            lastName,
            description,
            permissions.ToPermissionUpdateRequests(),
            defaultPasswordExpirationPeriod,
            firstPartition is null ? null : [firstPartition.Value],
            null);

    public static UserUpdateRequest ToUserUpdateRequest(
        string? nameid,
        string? firstName,
        string? lastName,
        string? description,
        string? password,
        bool? isActive,
        IReadOnlyCollection<ActionType>? permissions,
        TimeSpan? defaultPasswordExpirationPeriod)
        => new(
            nameid,
            firstName,
            lastName,
            description,
            password,
            isActive,
            permissions.ToPermissionUpdateRequests(),
            defaultPasswordExpirationPeriod,
            null,
            null);

    public static UserGroupCreateRequest ToUserGroupCreateRequest(
        string nameid,
        string? description,
        IReadOnlyCollection<ActionType>? permissions,
        Guid? firstPartition)
        => new(nameid, description, permissions.ToPermissionUpdateRequests(), firstPartition is null ? null : [firstPartition.Value]);

    public static UserGroupUpdateRequest ToUserGroupUpdateRequest(
        string? nameid,
        string? description,
        bool? isActive,
        IReadOnlyCollection<ActionType>? permissions)
        => new(nameid, description, isActive, permissions.ToPermissionUpdateRequests(), null);

    private static IReadOnlyCollection<PermissionUpdateRequest>? ToPermissionUpdateRequests(
        this IReadOnlyCollection<ActionType>? permissions)
        => permissions?.Select(static action => new PermissionUpdateRequest(action)).ToArray();

    private static ArticleMetricsInfo? ToContract(this ArticleMetrics? metrics)
        => metrics is null
            ? null
            : new ArticleMetricsInfo(
                metrics.Mass is null ? null : new MassInfo(metrics.Mass.Value, metrics.Mass.ToAbbreviation()),
                metrics.LengthX is null ? null : new LengthInfo(metrics.LengthX.Value, metrics.LengthX.ToAbbreviation()),
                metrics.LengthY is null ? null : new LengthInfo(metrics.LengthY.Value, metrics.LengthY.ToAbbreviation()),
                metrics.LengthZ is null ? null : new LengthInfo(metrics.LengthZ.Value, metrics.LengthZ.ToAbbreviation()));

    public static ArticleBarcodesInfo ToContract(this ArticleBarcodes barcodes)
        => new(
            barcodes.Ean13 is null ? null : barcodes.Ean13.Value.Code,
            barcodes.Ean8 is null ? null : barcodes.Ean8.Value.Code,
            barcodes.UpcA is null ? null : barcodes.UpcA.Value.Code,
            barcodes.UpcE is null ? null : barcodes.UpcE.Value.Code,
            barcodes.Code128 is null ? null : barcodes.Code128.Value.Code,
            barcodes.Code39 is null ? null : barcodes.Code39.Value.Code,
            barcodes.Itf14 is null ? null : barcodes.Itf14.Value.Code,
            barcodes.Gs1128 is null ? null : barcodes.Gs1128.Value.Code,
            barcodes.QrCode is null ? null : barcodes.QrCode.Value.Code,
            barcodes.DataMatrix is null ? null : barcodes.DataMatrix.Value.Code);
}
