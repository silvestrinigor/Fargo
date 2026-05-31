using Fargo.Application;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using System.Drawing;
using UnitsNet;
using UnitsNet.Units;
using AppArticle = Fargo.Application.Shared.Articles.ArticleDto;
using AppArticleBarcodes = Fargo.Application.Shared.Articles.ArticleBarcodesDto;
using AppArticleCreate = Fargo.Application.Shared.Articles.ArticleCreateDto;
using AppArticleCreateContainer = Fargo.Application.Shared.Articles.ArticleCreateContainerDto;
using AppArticleCreateKit = Fargo.Application.Shared.Articles.ArticleCreateKitDto;
using AppArticleCreateKitPack = Fargo.Application.Shared.Articles.ArticleCreateKitPackDto;
using AppArticleCreatePack = Fargo.Application.Shared.Articles.ArticleCreatePackDto;
using AppArticleCreateVariation = Fargo.Application.Shared.Articles.ArticleCreateVariationDto;
using AppArticleMetrics = Fargo.Application.Shared.Articles.ArticleMetricsDto;
using AppArticlePatch = Fargo.Application.Shared.Articles.ArticlePatchDto;
using AppAuth = Fargo.Application.Identity.AuthResult;
using AppItem = Fargo.Application.Items.ItemDto;
using AppItemCreate = Fargo.Application.Items.ItemCreateDto;
using AppItemUpdate = Fargo.Application.Items.ItemUpdateDto;
using AppPartition = Fargo.Application.Partitions.PartitionDto;
using AppPartitionCreate = Fargo.Application.Partitions.PartitionCreateDto;
using AppPartitionUpdate = Fargo.Application.Partitions.PartitionUpdateDto;
using AppUser = Fargo.Application.Users.UserDto;
using AppUserCreate = Fargo.Application.Users.UserCreateDto;
using AppUserGroup = Fargo.Application.UserGroups.UserGroupDto;
using AppUserGroupCreate = Fargo.Application.UserGroups.UserGroupCreateDto;
using AppUserGroupPermissionUpdate = Fargo.Application.UserGroups.UserGroupPermissionUpdateDto;
using AppUserGroupUpdate = Fargo.Application.UserGroups.UserGroupUpdateDto;
using AppUserPermissionUpdate = Fargo.Application.Users.UserPermissionUpdateDto;
using AppUserUpdate = Fargo.Application.Users.UserUpdateDto;

namespace Fargo.HttpApi.Contracts;

internal static class HttpContractMappings
{
    public static HttpContracts.AuthDto ToContract(this AppAuth result)
        => new(
            result.AccessToken.Value,
            result.RefreshToken.Value,
            result.ExpiresAt,
            result.IsAdmin,
            result.PermissionActions.Select(static action => (HttpContracts.ActionType)(int)action).ToArray(),
            result.PartitionAccesses);

    public static AppArticleCreate ToApplication(this HttpContracts.ArticleCreateRequest request)
        => new(
            new Name(request.Name),
            (Fargo.Core.Shared.Articles.ArticleType)(int)request.ArticleType,
            ToDescription(request.Description),
            request.ShelfLife,
            ToColor(request.Color),
            request.Metrics.ToApplication(),
            request.Barcodes.ToApplication(),
            request.Partitions,
            request.IsActive,
            request.Variation is null ? null : new AppArticleCreateVariation(request.Variation.FromArticleGuid),
            request.Pack is null ? null : new AppArticleCreatePack(
                request.Pack.FromArticleGuid,
                Scalar.FromAmount(request.Pack.Quantity)),
            request.Kit is null ? null : new AppArticleCreateKit(
                request.Kit.Packs.Select(static pack => new AppArticleCreateKitPack(
                    pack.ArticleGuid,
                    Scalar.FromAmount(pack.Quantity))).ToArray()),
            request.Container is null ? null : new AppArticleCreateContainer(
                request.Container.MaxMass.ToMass()));

    public static AppArticlePatch ToApplication(this HttpContracts.ArticlePatchRequest request)
        => new(
            ToName(request.Name),
            ToDescription(request.Description),
            request.ShelfLife,
            request.RemoveShelfLife,
            request.Metrics.ToApplication(),
            request.Barcodes.ToApplication(),
            request.Partitions,
            request.IsActive);

    public static HttpContracts.ArticleDto ToContract(this AppArticle article)
        => new(
            article.Guid,
            article.Name.Value,
            article.Description.Value,
            article.ShelfLife,
            article.Color is null ? null : ColorTranslator.ToHtml(article.Color.Value),
            article.Metrics.ToContract(),
            article.Barcodes.ToContract(),
            article.Partitions,
            article.IsActive,
            article.EditedByGuid);

    public static AppItemCreate ToApplication(this HttpContracts.ItemCreateRequest request)
        => new(
            request.ArticleGuid,
            request.ProductionDate,
            request.Partitions,
            request.IsActive);

    public static AppItemUpdate ToApplication(this HttpContracts.ItemUpdateRequest request)
        => new(
            request.Partitions,
            request.ParentContainerGuid,
            request.IsActive);

    public static HttpContracts.ItemDto ToContract(this AppItem item)
        => new(
            item.Guid,
            item.ArticleGuid,
            item.ProductionDate,
            item.ParentContainerGuid,
            item.Partitions,
            item.IsActive,
            item.EditedByGuid,
            (HttpContracts.ItemModifiedType)(int)item.ModificationTypes);

    public static AppUserCreate ToApplication(this HttpContracts.UserCreateRequest request)
        => new(
            request.Nameid,
            request.Password,
            ToFirstName(request.FirstName),
            ToLastName(request.LastName),
            ToDescription(request.Description),
            request.Permissions?.Select(static permission =>
                new AppUserPermissionUpdate((ActionType)(int)permission.Action)).ToArray(),
            request.DefaultPasswordExpirationTimeSpan,
            request.Partitions,
            request.UserGroups);

    public static AppUserUpdate ToApplication(this HttpContracts.UserUpdateRequest request)
        => new(
            request.Nameid,
            ToFirstName(request.FirstName),
            ToLastName(request.LastName),
            ToDescription(request.Description),
            request.Password,
            request.IsActive,
            request.Permissions?.Select(static permission =>
                new AppUserPermissionUpdate((ActionType)(int)permission.Action)).ToArray(),
            request.DefaultPasswordExpirationPeriod,
            request.Partitions,
            request.UserGroups);

    public static HttpContracts.UserDto ToContract(this AppUser user)
        => new(
            user.Guid,
            user.Nameid.Value,
            user.FirstName?.Value,
            user.LastName?.Value,
            user.Description.Value,
            user.DefaultPasswordExpirationPeriod,
            user.RequirePasswordChangeAt,
            user.Permissions.Select(static permission => new HttpContracts.PermissionDto(
                permission.Guid,
                (HttpContracts.ActionType)(int)permission.Action)).ToArray(),
            user.Partitions,
            user.UserGroups,
            user.IsActive,
            user.EditedByGuid,
            (HttpContracts.UserModifiedType)(int)user.ModificationTypes);

    public static AppUserGroupCreate ToApplication(this HttpContracts.UserGroupCreateRequest request)
        => new(
            request.Nameid,
            ToDescription(request.Description),
            request.Permissions?.Select(static permission =>
                new AppUserGroupPermissionUpdate((ActionType)(int)permission.Action)).ToArray(),
            request.Partitions);

    public static AppUserGroupUpdate ToApplication(this HttpContracts.UserGroupUpdateRequest request)
        => new(
            request.Nameid,
            ToDescription(request.Description),
            request.IsActive,
            request.Permissions?.Select(static permission =>
                new AppUserGroupPermissionUpdate((ActionType)(int)permission.Action)).ToArray(),
            request.Partitions);

    public static HttpContracts.UserGroupDto ToContract(this AppUserGroup userGroup)
        => new(
            userGroup.Guid,
            userGroup.Nameid.Value,
            userGroup.Description.Value,
            userGroup.Permissions.Select(static permission => new HttpContracts.PermissionDto(
                permission.Guid,
                (HttpContracts.ActionType)(int)permission.Action)).ToArray(),
            userGroup.Partitions,
            userGroup.IsActive,
            userGroup.EditedByGuid,
            (HttpContracts.UserGroupModifiedType)(int)userGroup.ModificationTypes);

    public static AppPartitionCreate ToApplication(this HttpContracts.PartitionCreateRequest request)
        => new(
            new Name(request.Name),
            ToDescription(request.Description),
            request.ParentPartitionGuid);

    public static AppPartitionUpdate ToApplication(this HttpContracts.PartitionUpdateRequest request)
        => new(
            ToName(request.Name),
            ToDescription(request.Description),
            request.ParentPartitionGuid,
            request.IsActive);

    public static HttpContracts.PartitionDto ToContract(this AppPartition partition)
        => new(
            partition.Guid,
            partition.Name.Value,
            partition.Description.Value,
            partition.ParentPartitionGuid,
            partition.IsActive,
            partition.EditedByGuid,
            (HttpContracts.PartitionModifiedType)(int)partition.ModificationTypes);

    private static AppArticleMetrics? ToApplication(this HttpContracts.ArticleMetricsDto? metrics)
        => metrics is null
            ? null
            : new AppArticleMetrics(
                metrics.Mass.ToMass(),
                metrics.LengthX.ToLength(),
                metrics.LengthY.ToLength(),
                metrics.LengthZ.ToLength());

    private static HttpContracts.ArticleMetricsDto ToContract(this AppArticleMetrics metrics)
        => new(
            metrics.Mass.ToContract(),
            metrics.LengthX.ToContract(),
            metrics.LengthY.ToContract(),
            metrics.LengthZ.ToContract());

    private static AppArticleBarcodes? ToApplication(this HttpContracts.ArticleBarcodesDto? barcodes)
        => barcodes is null
            ? null
            : new AppArticleBarcodes(
                barcodes.Ean13 is null ? null : new Ean13(barcodes.Ean13),
                barcodes.Ean8 is null ? null : new Ean8(barcodes.Ean8),
                barcodes.UpcA is null ? null : new UpcA(barcodes.UpcA),
                barcodes.UpcE is null ? null : new UpcE(barcodes.UpcE),
                barcodes.Code128 is null ? null : new Code128(barcodes.Code128),
                barcodes.Code39 is null ? null : new Code39(barcodes.Code39),
                barcodes.Itf14 is null ? null : new Itf14(barcodes.Itf14),
                barcodes.Gs1128 is null ? null : new Gs1128(barcodes.Gs1128),
                barcodes.QrCode is null ? null : new QrCode(barcodes.QrCode),
                barcodes.DataMatrix is null ? null : new DataMatrix(barcodes.DataMatrix));

    private static HttpContracts.ArticleBarcodesDto ToContract(this AppArticleBarcodes barcodes)
        => new(
            barcodes.Ean13?.Code,
            barcodes.Ean8?.Code,
            barcodes.UpcA?.Code,
            barcodes.UpcE?.Code,
            barcodes.Code128?.Code,
            barcodes.Code39?.Code,
            barcodes.Itf14?.Code,
            barcodes.Gs1128?.Code,
            barcodes.QrCode?.Code,
            barcodes.DataMatrix?.Code);

    private static HttpContracts.UnitValueDto? ToContract(this Mass? value)
        => value is null
            ? null
            : new HttpContracts.UnitValueDto(value.Value.Value, Mass.GetAbbreviation(value.Value.Unit));

    private static Mass? ToMass(this HttpContracts.UnitValueDto? value)
        => value is null
            ? null
            : Mass.From(value.Value, UnitParser.Default.Parse<MassUnit>(value.Unit));

    private static HttpContracts.UnitValueDto? ToContract(this Length? value)
        => value is null
            ? null
            : new HttpContracts.UnitValueDto(value.Value.Value, Length.GetAbbreviation(value.Value.Unit));

    private static Length? ToLength(this HttpContracts.UnitValueDto? value)
        => value is null
            ? null
            : Length.From(value.Value, UnitParser.Default.Parse<LengthUnit>(value.Unit));

    private static Name? ToName(string? value)
        => value is null ? null : new Name(value);

    private static Description? ToDescription(string? value)
        => value is null ? null : new Description(value);

    private static FirstName? ToFirstName(string? value)
        => value is null ? null : new FirstName(value);

    private static LastName? ToLastName(string? value)
        => value is null ? null : new LastName(value);

    private static Color? ToColor(string? value)
        => value is null ? null : ColorTranslator.FromHtml(value);
}
