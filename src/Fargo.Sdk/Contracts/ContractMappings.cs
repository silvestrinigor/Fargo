using Fargo.Sdk.Contracts.ApiClients;
using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Permissions;
using Fargo.Sdk.Contracts.UserGroups;
using Fargo.Sdk.Contracts.Users;

namespace Fargo.Sdk;

internal static class ContractMappings
{
    public static ApiClients.ApiClientResult ToSdk(this ApiClientDto contract)
        => new(contract.Guid, contract.Name, contract.Description, contract.IsActive, contract.EditedByGuid);

    public static IReadOnlyCollection<ApiClients.ApiClientResult> ToSdk(this IReadOnlyCollection<ApiClientDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static ApiClientCreateRequest ToApiClientCreateRequest(string name, string? description)
        => new(name, description);

    public static ApiClientUpdateRequest ToApiClientUpdateRequest(string? name, string? description, bool? isActive)
        => new(name, description, isActive);

    public static Articles.ArticleResult ToSdk(this ArticleDto contract)
        => new(
            contract.Guid,
            contract.Name,
            contract.Description,
            contract.Metrics.ToSdk(),
            contract.ShelfLife,
            contract.HasImage,
            contract.EditedByGuid,
            contract.Images is null ? null : new Articles.ArticleImages(contract.Images.HasImage),
            contract.Barcodes.ToSdk());

    public static IReadOnlyCollection<Articles.ArticleResult> ToSdk(this IReadOnlyCollection<ArticleDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static ArticleCreateRequest ToArticleCreateRequest(
        string name,
        string? description,
        Guid? firstPartition,
        Articles.ArticleMetrics? metrics,
        TimeSpan? shelfLife)
        => new(new ArticleCreateDto(name, description, firstPartition, metrics.ToContract(), shelfLife));

    public static ArticleUpdateRequest ToArticleUpdateRequest(
        string? name,
        string? description,
        Articles.ArticleMetrics? metrics,
        TimeSpan? shelfLife)
        => new(name, description, metrics.ToContract(), shelfLife);

    public static Items.ItemResult ToSdk(this ItemDto contract)
        => new(contract.Guid, contract.ArticleGuid, contract.ProductionDate, contract.EditedByGuid);

    public static IReadOnlyCollection<Items.ItemResult> ToSdk(this IReadOnlyCollection<ItemDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static ItemCreateRequest ToItemCreateRequest(Guid articleGuid, Guid? firstPartition, DateTimeOffset? productionDate)
        => new(new ItemCreateDto(articleGuid, firstPartition, productionDate));

    public static ItemUpdateRequest ToItemUpdateRequest(DateTimeOffset? productionDate)
        => new(productionDate);

    public static Partitions.PartitionResult ToSdk(this PartitionDto contract)
        => new(
            contract.Guid,
            contract.Name,
            contract.Description,
            contract.ParentPartitionGuid,
            contract.IsActive,
            contract.EditedByGuid);

    public static IReadOnlyCollection<Partitions.PartitionResult> ToSdk(this IReadOnlyCollection<PartitionDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

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

    public static Users.UserResult ToSdk(this UserDto contract)
        => new(
            contract.Guid,
            contract.Nameid,
            contract.FirstName,
            contract.LastName,
            contract.Description,
            contract.DefaultPasswordExpirationPeriod,
            contract.RequirePasswordChangeAt,
            contract.IsActive,
            contract.Permissions.Select(static x => x.ToSdk()).ToArray(),
            contract.PartitionAccesses.ToArray(),
            contract.EditedByGuid);

    public static IReadOnlyCollection<Users.UserResult> ToSdk(this IReadOnlyCollection<UserDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static UserCreateRequest ToUserCreateRequest(
        string nameid,
        string password,
        string? firstName,
        string? lastName,
        string? description,
        IReadOnlyCollection<ActionType>? permissions,
        TimeSpan? defaultPasswordExpirationPeriod,
        Guid? firstPartition)
        => new(new UserCreateDto(
            nameid,
            password,
            firstName,
            lastName,
            description,
            permissions.ToPermissionUpdateRequests(),
            defaultPasswordExpirationPeriod,
            firstPartition));

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
            defaultPasswordExpirationPeriod);

    public static UserGroups.UserGroupResult ToSdk(this UserGroupDto contract)
        => new(
            contract.Guid,
            contract.Nameid,
            contract.Description,
            contract.IsActive,
            contract.Permissions.Select(static x => x.ToSdk()).ToArray());

    public static IReadOnlyCollection<UserGroups.UserGroupResult> ToSdk(this IReadOnlyCollection<UserGroupDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static UserGroupCreateRequest ToUserGroupCreateRequest(
        string nameid,
        string? description,
        IReadOnlyCollection<ActionType>? permissions,
        Guid? firstPartition)
        => new(new UserGroupCreateDto(nameid, description, permissions.ToPermissionUpdateRequests(), firstPartition));

    public static UserGroupUpdateRequest ToUserGroupUpdateRequest(
        string? nameid,
        string? description,
        bool? isActive,
        IReadOnlyCollection<ActionType>? permissions)
        => new(nameid, description, isActive, permissions.ToPermissionUpdateRequests());

    private static PermissionResult ToSdk(this PermissionDto contract)
        => new(contract.Guid, contract.Action);

    private static IReadOnlyCollection<PermissionUpdateRequest>? ToPermissionUpdateRequests(
        this IReadOnlyCollection<ActionType>? permissions)
        => permissions?.Select(static action => new PermissionUpdateRequest(action)).ToArray();

    private static ArticleMetricsDto? ToContract(this Articles.ArticleMetrics? metrics)
        => metrics is null
            ? null
            : new ArticleMetricsDto(
                metrics.Mass is null ? null : new MassDto(metrics.Mass.Value, metrics.Mass.ToAbbreviation()),
                metrics.LengthX is null ? null : new LengthDto(metrics.LengthX.Value, metrics.LengthX.ToAbbreviation()),
                metrics.LengthY is null ? null : new LengthDto(metrics.LengthY.Value, metrics.LengthY.ToAbbreviation()),
                metrics.LengthZ is null ? null : new LengthDto(metrics.LengthZ.Value, metrics.LengthZ.ToAbbreviation()));

    private static Articles.ArticleMetrics? ToSdk(this ArticleMetricsDto? contract)
        => contract is null
            ? null
            : new Articles.ArticleMetrics
            {
                Mass = contract.Mass is null ? null : new Articles.Mass(contract.Mass.Value, Articles.Mass.ParseUnit(contract.Mass.Unit)),
                LengthX = contract.LengthX is null ? null : new Articles.Length(contract.LengthX.Value, Articles.Length.ParseUnit(contract.LengthX.Unit)),
                LengthY = contract.LengthY is null ? null : new Articles.Length(contract.LengthY.Value, Articles.Length.ParseUnit(contract.LengthY.Unit)),
                LengthZ = contract.LengthZ is null ? null : new Articles.Length(contract.LengthZ.Value, Articles.Length.ParseUnit(contract.LengthZ.Unit)),
            };

    public static ArticleBarcodesDto ToContract(this Articles.ArticleBarcodes barcodes)
        => new(
            barcodes.Ean13 is null ? null : new BarcodeValueDto(barcodes.Ean13.Value.Code),
            barcodes.Ean8 is null ? null : new BarcodeValueDto(barcodes.Ean8.Value.Code),
            barcodes.UpcA is null ? null : new BarcodeValueDto(barcodes.UpcA.Value.Code),
            barcodes.UpcE is null ? null : new BarcodeValueDto(barcodes.UpcE.Value.Code),
            barcodes.Code128 is null ? null : new BarcodeValueDto(barcodes.Code128.Value.Code),
            barcodes.Code39 is null ? null : new BarcodeValueDto(barcodes.Code39.Value.Code),
            barcodes.Itf14 is null ? null : new BarcodeValueDto(barcodes.Itf14.Value.Code),
            barcodes.Gs1128 is null ? null : new BarcodeValueDto(barcodes.Gs1128.Value.Code),
            barcodes.QrCode is null ? null : new BarcodeValueDto(barcodes.QrCode.Value.Code),
            barcodes.DataMatrix is null ? null : new BarcodeValueDto(barcodes.DataMatrix.Value.Code));

    public static Articles.ArticleBarcodes ToSdk(this ArticleBarcodesDto? contract)
        => contract is null
            ? new Articles.ArticleBarcodes()
            : new Articles.ArticleBarcodes
            {
                Ean13 = contract.Ean13 is null ? null : new Articles.Ean13(Guid.Empty, Guid.Empty, contract.Ean13.Code),
                Ean8 = contract.Ean8 is null ? null : new Articles.Ean8(Guid.Empty, Guid.Empty, contract.Ean8.Code),
                UpcA = contract.UpcA is null ? null : new Articles.UpcA(Guid.Empty, Guid.Empty, contract.UpcA.Code),
                UpcE = contract.UpcE is null ? null : new Articles.UpcE(Guid.Empty, Guid.Empty, contract.UpcE.Code),
                Code128 = contract.Code128 is null ? null : new Articles.Code128(Guid.Empty, Guid.Empty, contract.Code128.Code),
                Code39 = contract.Code39 is null ? null : new Articles.Code39(Guid.Empty, Guid.Empty, contract.Code39.Code),
                Itf14 = contract.Itf14 is null ? null : new Articles.Itf14(Guid.Empty, Guid.Empty, contract.Itf14.Code),
                Gs1128 = contract.Gs1128 is null ? null : new Articles.Gs1128(Guid.Empty, Guid.Empty, contract.Gs1128.Code),
                QrCode = contract.QrCode is null ? null : new Articles.QrCode(Guid.Empty, Guid.Empty, contract.QrCode.Code),
                DataMatrix = contract.DataMatrix is null ? null : new Articles.DataMatrix(Guid.Empty, Guid.Empty, contract.DataMatrix.Code),
            };
}
