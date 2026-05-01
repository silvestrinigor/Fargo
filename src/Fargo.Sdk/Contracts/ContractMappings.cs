using Fargo.Api;
using Fargo.Api.Contracts.ApiClients;
using Fargo.Api.Contracts.Articles;
using Fargo.Api.Contracts.Items;
using Fargo.Api.Contracts.Partitions;
using Fargo.Api.Contracts.Permissions;
using Fargo.Api.Contracts.UserGroups;
using Fargo.Api.Contracts.Users;
using ApiClients = Fargo.Api.ApiClients;
using Articles = Fargo.Api.Articles;
using Items = Fargo.Api.Items;
using Partitions = Fargo.Api.Partitions;
using UserGroups = Fargo.Api.UserGroups;
using Users = Fargo.Api.Users;

namespace Fargo.Sdk;

internal static class ContractMappings
{
    public static ApiClients.ApiClientResult ToSdk(this ApiClientDto contract)
        => new(contract.Guid, contract.Name, contract.Description, contract.IsActive, contract.EditedByGuid);

    public static IReadOnlyCollection<ApiClients.ApiClientResult> ToSdk(this IReadOnlyCollection<ApiClientDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static ApiClientCreateDto ToApiClientCreateDto(string name, string? description)
        => new(name, description);

    public static ApiClientUpdateDto ToApiClientUpdateDto(string? name, string? description, bool? isActive)
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

    public static ArticleCreateDto ToArticleCreateDto(
        string name,
        string? description,
        Guid? firstPartition,
        Articles.ArticleMetrics? metrics,
        TimeSpan? shelfLife)
        => new(name, description, firstPartition, metrics.ToContract(), shelfLife);

    public static ArticleUpdateDto ToArticleUpdateDto(
        string? name,
        string? description,
        Articles.ArticleMetrics? metrics,
        TimeSpan? shelfLife)
        => new(name, description, metrics.ToContract(), shelfLife);

    public static Items.ItemResult ToSdk(this ItemDto contract)
        => new(contract.Guid, contract.ArticleGuid, contract.ProductionDate, contract.EditedByGuid);

    public static IReadOnlyCollection<Items.ItemResult> ToSdk(this IReadOnlyCollection<ItemDto> contracts)
        => contracts.Select(static x => x.ToSdk()).ToArray();

    public static ItemCreateDto ToItemCreateDto(Guid articleGuid, Guid? firstPartition, DateTimeOffset? productionDate)
        => new(articleGuid, firstPartition, productionDate);

    public static ItemUpdateDto ToItemUpdateDto(DateTimeOffset? productionDate)
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

    public static PartitionCreateDto ToPartitionCreateDto(
        string name,
        string? description,
        Guid? parentPartitionGuid)
        => new(name, description, parentPartitionGuid);

    public static PartitionUpdateDto ToPartitionUpdateDto(
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

    public static UserCreateDto ToUserCreateDto(
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
            permissions.ToPermissionUpdateDtos(),
            defaultPasswordExpirationPeriod,
            firstPartition);

    public static UserUpdateDto ToUserUpdateDto(
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
            permissions.ToPermissionUpdateDtos(),
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

    public static UserGroupCreateDto ToUserGroupCreateDto(
        string nameid,
        string? description,
        IReadOnlyCollection<ActionType>? permissions,
        Guid? firstPartition)
        => new(nameid, description, permissions.ToPermissionUpdateDtos(), firstPartition);

    public static UserGroupUpdateDto ToUserGroupUpdateDto(
        string? nameid,
        string? description,
        bool? isActive,
        IReadOnlyCollection<ActionType>? permissions)
        => new(nameid, description, isActive, permissions.ToPermissionUpdateDtos());

    private static PermissionResult ToSdk(this PermissionDto contract)
        => new(contract.Guid, contract.Action);

    private static IReadOnlyCollection<PermissionUpdateDto>? ToPermissionUpdateDtos(
        this IReadOnlyCollection<ActionType>? permissions)
        => permissions?.Select(static action => new PermissionUpdateDto(action)).ToArray();

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

    public static Articles.ArticleBarcodes ToSdk(this ArticleBarcodesDto? contract)
        => contract is null
            ? new Articles.ArticleBarcodes()
            : new Articles.ArticleBarcodes
            {
                Ean13 = contract.Ean13 is null ? null : new Articles.Ean13(Guid.Empty, Guid.Empty, contract.Ean13),
                Ean8 = contract.Ean8 is null ? null : new Articles.Ean8(Guid.Empty, Guid.Empty, contract.Ean8),
                UpcA = contract.UpcA is null ? null : new Articles.UpcA(Guid.Empty, Guid.Empty, contract.UpcA),
                UpcE = contract.UpcE is null ? null : new Articles.UpcE(Guid.Empty, Guid.Empty, contract.UpcE),
                Code128 = contract.Code128 is null ? null : new Articles.Code128(Guid.Empty, Guid.Empty, contract.Code128),
                Code39 = contract.Code39 is null ? null : new Articles.Code39(Guid.Empty, Guid.Empty, contract.Code39),
                Itf14 = contract.Itf14 is null ? null : new Articles.Itf14(Guid.Empty, Guid.Empty, contract.Itf14),
                Gs1128 = contract.Gs1128 is null ? null : new Articles.Gs1128(Guid.Empty, Guid.Empty, contract.Gs1128),
                QrCode = contract.QrCode is null ? null : new Articles.QrCode(Guid.Empty, Guid.Empty, contract.QrCode),
                DataMatrix = contract.DataMatrix is null ? null : new Articles.DataMatrix(Guid.Empty, Guid.Empty, contract.DataMatrix),
            };
}
