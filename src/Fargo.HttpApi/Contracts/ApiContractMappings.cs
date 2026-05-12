using Fargo.Application.Authentication;
using Fargo.Core;
using Fargo.Core.Barcodes;
using Fargo.Core.Users;
using UnitsNet;
using UnitsNet.Units;
using AppArticles = Fargo.Application.Articles;
using AppEvents = Fargo.Application.Events;
using AppItems = Fargo.Application.Items;
using AppPartitions = Fargo.Application.Partitions;
using AppUserGroups = Fargo.Application.UserGroups;
using AppUsers = Fargo.Application.Users;
using ContractActionType = Fargo.Sdk.Contracts.ActionType;
using ContractArticles = Fargo.Sdk.Contracts.Articles;
using ContractAuthentication = Fargo.Sdk.Contracts.Authentication;
using ContractEntityType = Fargo.Sdk.Contracts.EntityType;
using ContractEvents = Fargo.Sdk.Contracts.Events;
using ContractItems = Fargo.Sdk.Contracts.Items;
using ContractPartitions = Fargo.Sdk.Contracts.Partitions;
using ContractPermissions = Fargo.Sdk.Contracts.Permissions;
using ContractUserGroups = Fargo.Sdk.Contracts.UserGroups;
using ContractUsers = Fargo.Sdk.Contracts.Users;
using DomainActionType = Fargo.Core.ActionType;
using DomainBarcodeFormat = Fargo.Core.Barcodes.BarcodeFormat;
using DomainEntityType = Fargo.Core.Events.EntityType;
using DomainEventType = Fargo.Core.Events.EventType;

namespace Fargo.HttpApi.Contracts;

internal static class ApiContractMappings
{
    public static ContractArticles.ArticleInfo ToInfo(this AppArticles.ArticleDto article)
        => new(
            article.Guid,
            article.Name,
            article.Description,
            article.Metrics.ToInfo(),
            article.ShelfLife,
            article.Ean13,
            article.Ean8,
            article.UpcA,
            article.UpcE,
            article.Code128,
            article.Code39,
            article.Itf14,
            article.Gs1128,
            article.QrCode,
            article.DataMatrix,
            article.Partitions,
            article.IsActive,
            article.EditedByGuid);

    public static IReadOnlyCollection<ContractArticles.ArticleInfo> ToInfo(this IReadOnlyCollection<AppArticles.ArticleDto> articles)
        => articles.Select(static article => article.ToInfo()).ToArray();

    public static AppArticles.ArticleCreateDto ToApplicationDto(this ContractArticles.ArticleCreateRequest request)
        => new(
            new Name(request.Name),
            request.Description.ToDescription(),
            request.ShelfLife,
            request.Metrics.ToApplicationDto(),
            request.Ean13.ToEan13ApplicationDto(),
            request.Ean8.ToEan8ApplicationDto(),
            request.UpcA.ToUpcAApplicationDto(),
            request.UpcE.ToUpcEApplicationDto(),
            request.Code128.ToCode128ApplicationDto(),
            request.Code39.ToCode39ApplicationDto(),
            request.Itf14.ToItf14ApplicationDto(),
            request.Gs1128.ToGs1128ApplicationDto(),
            request.QrCode.ToQrCodeApplicationDto(),
            request.DataMatrix.ToDataMatrixApplicationDto(),
            request.Partitions,
            request.IsActive);

    public static AppArticles.ArticleUpdateDto ToApplicationDto(this ContractArticles.ArticleUpdateRequest request)
        => new(
            new Name(request.Name),
            request.Description.ToDescription() ?? Description.Empty,
            request.ShelfLife,
            request.Metrics.ToApplicationDto() ?? new AppArticles.ArticleMetricsDto(),
            request.Ean13.ToEan13ApplicationDto(),
            request.Ean8.ToEan8ApplicationDto(),
            request.UpcA.ToUpcAApplicationDto(),
            request.UpcE.ToUpcEApplicationDto(),
            request.Code128.ToCode128ApplicationDto(),
            request.Code39.ToCode39ApplicationDto(),
            request.Itf14.ToItf14ApplicationDto(),
            request.Gs1128.ToGs1128ApplicationDto(),
            request.QrCode.ToQrCodeApplicationDto(),
            request.DataMatrix.ToDataMatrixApplicationDto(),
            request.Partitions ?? [],
            request.IsActive);

    public static AppArticles.ArticleBarcodeDto ToApplicationDto(this ContractArticles.ArticleBarcode articleBarcode)
        => new(articleBarcode.Barcode, articleBarcode.Type.ToDomain());

    public static ContractItems.ItemInfo ToInfo(this AppItems.ItemDto item)
        => new(
            item.Guid,
            item.ArticleGuid,
            item.ProductionDate,
            item.ParentContainerGuid,
            item.Partitions,
            item.EditedByGuid);

    public static IReadOnlyCollection<ContractItems.ItemInfo> ToInfo(this IReadOnlyCollection<AppItems.ItemDto> items)
        => items.Select(static item => item.ToInfo()).ToArray();

    public static AppItems.ItemCreateDto ToApplicationDto(this ContractItems.ItemCreateRequest request)
        => new(request.ArticleGuid, request.ProductionDate, request.Partitions);

    public static AppItems.ItemUpdateDto ToApplicationDto(this ContractItems.ItemUpdateRequest request)
        => new(request.Partitions, request.ParentContainerGuid);

    public static ContractPartitions.PartitionInfo ToInfo(this AppPartitions.PartitionDto partition)
        => new(
            partition.Guid,
            partition.Name,
            partition.Description,
            partition.ParentPartitionGuid,
            partition.IsActive,
            partition.EditedByGuid);

    public static IReadOnlyCollection<ContractPartitions.PartitionInfo> ToInfo(this IReadOnlyCollection<AppPartitions.PartitionDto> partitions)
        => partitions.Select(static partition => partition.ToInfo()).ToArray();

    public static AppPartitions.PartitionCreateDto ToApplicationDto(this ContractPartitions.PartitionCreateRequest request)
        => new(new Name(request.Name), request.Description.ToDescription(), request.ParentPartitionGuid);

    public static AppPartitions.PartitionUpdateDto ToApplicationDto(this ContractPartitions.PartitionUpdateRequest request)
        => new(request.Name.ToName(), request.Description.ToDescription(), request.ParentPartitionGuid, request.IsActive);

    public static ContractUsers.UserInfo ToInfo(this AppUsers.UserDto user)
        => new(
            user.Guid,
            user.Nameid,
            user.FirstName,
            user.LastName,
            user.Description,
            user.DefaultPasswordExpirationPeriod,
            user.RequirePasswordChangeAt,
            user.IsActive,
            user.Permissions.ToInfo(),
            user.Partitions,
            user.UserGroups,
            user.EditedByGuid);

    public static IReadOnlyCollection<ContractUsers.UserInfo> ToInfo(this IReadOnlyCollection<AppUsers.UserDto> users)
        => users.Select(static user => user.ToInfo()).ToArray();

    public static AppUsers.UserCreateDto ToApplicationDto(this ContractUsers.UserCreateRequest request)
        => new(
            request.Nameid,
            request.Password,
            request.FirstName.ToFirstName(),
            request.LastName.ToLastName(),
            request.Description.ToDescription(),
            request.Permissions.ToUserPermissionUpdateDtos(),
            request.DefaultPasswordExpirationTimeSpan,
            request.Partitions,
            request.UserGroups);

    public static AppUsers.UserUpdateDto ToApplicationDto(this ContractUsers.UserUpdateRequest request)
        => new(
            request.Nameid,
            request.FirstName.ToFirstName(),
            request.LastName.ToLastName(),
            request.Description.ToDescription(),
            request.Password,
            request.IsActive,
            request.Permissions.ToUserPermissionUpdateDtos(),
            request.DefaultPasswordExpirationPeriod,
            request.Partitions,
            request.UserGroups);

    public static ContractUserGroups.UserGroupInfo ToInfo(this AppUserGroups.UserGroupDto userGroup)
        => new(
            userGroup.Guid,
            userGroup.Nameid,
            userGroup.Description,
            userGroup.IsActive,
            userGroup.Permissions.ToInfo(),
            userGroup.Partitions,
            userGroup.EditedByGuid);

    public static IReadOnlyCollection<ContractUserGroups.UserGroupInfo> ToInfo(this IReadOnlyCollection<AppUserGroups.UserGroupDto> userGroups)
        => userGroups.Select(static userGroup => userGroup.ToInfo()).ToArray();

    public static AppUserGroups.UserGroupCreateDto ToApplicationDto(this ContractUserGroups.UserGroupCreateRequest request)
        => new(
            request.Nameid,
            request.Description.ToDescription(),
            request.Permissions.ToUserGroupPermissionUpdateDtos(),
            request.Partitions);

    public static AppUserGroups.UserGroupUpdateDto ToApplicationDto(this ContractUserGroups.UserGroupUpdateRequest request)
        => new(
            request.Nameid,
            request.Description.ToDescription(),
            request.IsActive,
            request.Permissions.ToUserGroupPermissionUpdateDtos(),
            request.Partitions);

    public static ContractAuthentication.AuthInfo ToInfo(this AuthResult result)
        => new(
            result.AccessToken.Value,
            result.RefreshToken.Value,
            result.ExpiresAt,
            result.IsAdmin,
            result.PermissionActions.Select(static action => (ContractActionType)(int)action).ToArray(),
            result.PartitionAccesses);

    public static ContractEvents.EventInfo ToInfo(this AppEvents.EventInformation eventInformation)
        => new(
            eventInformation.Guid,
            (ContractEvents.EventType)(int)eventInformation.EventType,
            (ContractEntityType)(int)eventInformation.EntityType,
            eventInformation.EntityGuid,
            eventInformation.ActorGuid,
            eventInformation.OccurredAt);

    public static IReadOnlyCollection<ContractEvents.EventInfo> ToInfo(this IReadOnlyCollection<AppEvents.EventInformation> events)
        => events.Select(static eventInformation => eventInformation.ToInfo()).ToArray();

    public static DomainEntityType ToDomain(this ContractEntityType entityType)
        => (DomainEntityType)(int)entityType;

    public static DomainEventType ToDomain(this ContractEvents.EventType eventType)
        => (DomainEventType)(int)eventType;

    private static DomainBarcodeFormat ToDomain(this ContractArticles.ArticleBarcodeType barcodeType)
        => barcodeType switch
        {
            ContractArticles.ArticleBarcodeType.Ean13 => DomainBarcodeFormat.Ean13,
            ContractArticles.ArticleBarcodeType.Ean8 => DomainBarcodeFormat.Ean8,
            ContractArticles.ArticleBarcodeType.UpcA => DomainBarcodeFormat.UpcA,
            ContractArticles.ArticleBarcodeType.UpcE => DomainBarcodeFormat.UpcE,
            ContractArticles.ArticleBarcodeType.Code128 => DomainBarcodeFormat.Code128,
            ContractArticles.ArticleBarcodeType.Code39 => DomainBarcodeFormat.Code39,
            ContractArticles.ArticleBarcodeType.Itf14 => DomainBarcodeFormat.Itf14,
            ContractArticles.ArticleBarcodeType.Gs1128 => DomainBarcodeFormat.Gs1128,
            ContractArticles.ArticleBarcodeType.QrCode => DomainBarcodeFormat.QrCode,
            ContractArticles.ArticleBarcodeType.DataMatrix => DomainBarcodeFormat.DataMatrix,
            _ => throw new ArgumentOutOfRangeException(nameof(barcodeType), barcodeType, "Unsupported barcode type.")
        };

    private static IReadOnlyCollection<ContractPermissions.PermissionInfo> ToInfo(this IReadOnlyCollection<Permission> permissions)
        => permissions.Select(static permission => new ContractPermissions.PermissionInfo(
            permission.Guid,
            (ContractActionType)(int)permission.Action)).ToArray();

    private static IReadOnlyCollection<AppUsers.UserPermissionUpdateDto>? ToUserPermissionUpdateDtos(
        this IReadOnlyCollection<ContractPermissions.PermissionUpdateRequest>? permissions)
        => permissions?.Select(static permission => new AppUsers.UserPermissionUpdateDto(
            (DomainActionType)(int)permission.Action)).ToArray();

    private static IReadOnlyCollection<AppUserGroups.UserGroupPermissionUpdateDto>? ToUserGroupPermissionUpdateDtos(
        this IReadOnlyCollection<ContractPermissions.PermissionUpdateRequest>? permissions)
        => permissions?.Select(static permission => new AppUserGroups.UserGroupPermissionUpdateDto(
            (DomainActionType)(int)permission.Action)).ToArray();

    private static ContractArticles.ArticleMetricsInfo ToInfo(this AppArticles.ArticleMetricsDto metrics)
        => new(
            metrics.Mass.ToInfo(),
            metrics.LengthX.ToInfo(),
            metrics.LengthY.ToInfo(),
            metrics.LengthZ.ToInfo());

    private static AppArticles.ArticleMetricsDto? ToApplicationDto(this ContractArticles.ArticleMetricsInfo? metrics)
        => metrics is null
            ? null
            : new AppArticles.ArticleMetricsDto(
                metrics.Mass.ToMass(),
                metrics.LengthX.ToLength(),
                metrics.LengthY.ToLength(),
                metrics.LengthZ.ToLength());

    private static ContractArticles.MassInfo? ToInfo(this Mass? mass)
        => mass is not { } value ? null : new ContractArticles.MassInfo(value.Value, Mass.GetAbbreviation(value.Unit));

    private static Mass? ToMass(this ContractArticles.MassInfo? mass)
        => mass is null ? null : Mass.From(mass.Value, UnitParser.Default.Parse<MassUnit>(mass.Unit));

    private static ContractArticles.LengthInfo? ToInfo(this Length? length)
        => length is not { } value ? null : new ContractArticles.LengthInfo(value.Value, Length.GetAbbreviation(value.Unit));

    private static Length? ToLength(this ContractArticles.LengthInfo? length)
        => length is null ? null : Length.From(length.Value, UnitParser.Default.Parse<LengthUnit>(length.Unit));

    private static Ean13? ToEan13ApplicationDto(this string? barcode)
        => barcode is null ? null : new Ean13(barcode);

    private static Ean8? ToEan8ApplicationDto(this string? barcode)
        => barcode is null ? null : new Ean8(barcode);

    private static UpcA? ToUpcAApplicationDto(this string? barcode)
        => barcode is null ? null : new UpcA(barcode);

    private static UpcE? ToUpcEApplicationDto(this string? barcode)
        => barcode is null ? null : new UpcE(barcode);

    private static Code128? ToCode128ApplicationDto(this string? barcode)
        => barcode is null ? null : new Code128(barcode);

    private static Code39? ToCode39ApplicationDto(this string? barcode)
        => barcode is null ? null : new Code39(barcode);

    private static Itf14? ToItf14ApplicationDto(this string? barcode)
        => barcode is null ? null : new Itf14(barcode);

    private static Gs1128? ToGs1128ApplicationDto(this string? barcode)
        => barcode is null ? null : new Gs1128(barcode);

    private static QrCode? ToQrCodeApplicationDto(this string? barcode)
        => barcode is null ? null : new QrCode(barcode);

    private static DataMatrix? ToDataMatrixApplicationDto(this string? barcode)
        => barcode is null ? null : new DataMatrix(barcode);

    private static Name? ToName(this string? value)
        => value is null ? null : new Name(value);

    private static Description? ToDescription(this string? value)
        => value is null ? null : new Description(value);

    private static FirstName? ToFirstName(this string? value)
        => value is null ? null : new FirstName(value);

    private static LastName? ToLastName(this string? value)
        => value is null ? null : new LastName(value);
}
