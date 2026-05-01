using Fargo.Api.Contracts.ApiClients;
using Fargo.Api.Contracts.Articles;
using Fargo.Api.Contracts.Authentication;
using Fargo.Api.Contracts.Items;
using Fargo.Api.Contracts.Partitions;
using Fargo.Api.Contracts.Permissions;
using Fargo.Api.Contracts.UserGroups;
using Fargo.Api.Contracts.Users;
using Fargo.Application.ApiClients;
using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Users;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Api.Contracts;

internal static class ContractMappings
{
    public static AuthDto ToContract(this AuthResult result)
        => new(
            result.AccessToken.Value,
            result.RefreshToken.Value,
            result.ExpiresAt,
            result.IsAdmin,
            result.PermissionActions.Select(static action => (Fargo.Api.Contracts.ActionType)(int)action).ToArray(),
            result.PartitionAccesses.ToArray());

    public static ApiClientCreatedDto ToContract(this ApiClientCreatedResult result)
        => new(result.Guid, result.PlainKey);

    public static LoginCommand ToCommand(this LoginDto request)
        => new(request.Nameid, request.Password);

    public static LogoutCommand ToLogoutCommand(this RefreshDto request)
        => new(new Fargo.Domain.Tokens.Token(request.RefreshToken));

    public static RefreshCommand ToRefreshCommand(this RefreshDto request)
        => new(new Fargo.Domain.Tokens.Token(request.RefreshToken));

    public static PasswordChangeCommand ToCommand(this PasswordUpdateDto request)
        => new(new UserPasswordUpdateModel(request.NewPassword, request.CurrentPassword));

    public static ApiClientCreateCommand ToCommand(this ApiClientCreateDto request)
        => new(request.Name, request.Description);

    public static ApiClientUpdateCommand ToCommand(this ApiClientUpdateDto request, Guid guid)
        => new(guid, request.Name, request.Description, request.IsActive);

    public static ApiClientDto ToContract(this ApiClientInformation info)
        => new(info.Guid, info.Name, info.Description, info.IsActive, info.EditedByGuid);

    public static ArticleCreateCommand ToCommand(this ArticleCreateDto request)
        => new(new ArticleCreateModel(
            new Name(request.Name),
            ToDescription(request.Description),
            request.FirstPartition,
            request.Metrics.ToApplicationModel(),
            request.ShelfLife));

    public static ArticleUpdateCommand ToCommand(this ArticleUpdateDto request, Guid articleGuid)
        => new(articleGuid, new ArticleUpdateModel(
            ToName(request.Name),
            ToDescription(request.Description),
            request.Metrics.ToApplicationModel(),
            request.ShelfLife));

    public static ArticleUpdateBarcodesCommand ToCommand(this ArticleBarcodesDto request, Guid articleGuid)
        => new(articleGuid, request.ToDomain());

    public static ArticleDto ToContract(this ArticleInformation info)
        => new(
            info.Guid,
            info.Name.ToString(),
            info.Description.ToString(),
            info.Metrics.ToContract(),
            info.ShelfLife,
            info.HasImage,
            info.EditedByGuid,
            info.Images.ToContract(),
            info.Barcodes.ToContract());

    public static ItemCreateCommand ToCommand(this ItemCreateDto request)
        => new(new ItemCreateModel(
            request.ArticleGuid,
            request.FirstPartition,
            request.ProductionDate));

    public static ItemUpdateCommand ToCommand(this ItemUpdateDto request, Guid itemGuid)
        => new(itemGuid, new ItemUpdateModel(request.ProductionDate));

    public static ItemDto ToContract(this ItemInformation info)
        => new(info.Guid, info.ArticleGuid, info.ProductionDate, info.EditedByGuid);

    public static PartitionCreateCommand ToCommand(this PartitionCreateDto request)
        => new(new Name(request.Name), ToDescription(request.Description), request.ParentPartitionGuid);

    public static PartitionUpdateCommand ToCommand(this PartitionUpdateDto request, Guid partitionGuid)
        => new(partitionGuid, new PartitionUpdateModel(
            ToName(request.Name),
            ToDescription(request.Description),
            request.ParentPartitionGuid,
            request.IsActive));

    public static PartitionDto ToContract(this PartitionInformation info)
        => new(
            info.Guid,
            info.Name.ToString(),
            info.Description.ToString(),
            info.ParentPartitionGuid,
            info.IsActive,
            info.EditedByGuid);

    public static UserCreateCommand ToCommand(this UserCreateDto request)
        => new(new UserCreateModel(
            request.Nameid,
            request.Password,
            ToFirstName(request.FirstName),
            ToLastName(request.LastName),
            ToDescription(request.Description),
            request.Permissions.ToUserPermissionModels(),
            request.DefaultPasswordExpirationTimeSpan,
            request.FirstPartition));

    public static UserUpdateCommand ToCommand(this UserUpdateDto request, Guid userGuid)
        => new(userGuid, new UserUpdateModel(
            request.Nameid,
            ToFirstName(request.FirstName),
            ToLastName(request.LastName),
            ToDescription(request.Description),
            request.Password,
            request.IsActive,
            request.Permissions.ToUserPermissionModels(),
            request.DefaultPasswordExpirationPeriod));

    public static UserDto ToContract(this UserInformation info)
        => new(
            info.Guid,
            info.Nameid.ToString(),
            info.FirstName?.ToString(),
            info.LastName?.ToString(),
            info.Description.ToString(),
            info.DefaultPasswordExpirationPeriod,
            info.RequirePasswordChangeAt,
            info.IsActive,
            info.Permissions.Select(static p => p.ToContract()).ToArray(),
            info.PartitionAccesses.ToArray(),
            info.EditedByGuid);

    public static UserGroupCreateCommand ToCommand(this UserGroupCreateDto request)
        => new(new UserGroupCreateModel(
            request.Nameid,
            ToDescription(request.Description),
            request.Permissions.ToUserGroupPermissionModels(),
            request.FirstPartition));

    public static UserGroupUpdateCommand ToCommand(this UserGroupUpdateDto request, Guid userGroupGuid)
        => new(userGroupGuid, new UserGroupUpdateModel(
            request.Nameid,
            ToDescription(request.Description),
            request.IsActive,
            request.Permissions.ToUserGroupPermissionModels()));

    public static UserGroupDto ToContract(this UserGroupInformation info)
        => new(
            info.Guid,
            info.Nameid.ToString(),
            info.Description.ToString(),
            info.IsActive,
            info.Permissions.Select(static p => p.ToContract()).ToArray());

    public static Fargo.Api.Contracts.Events.EventDto ToContract(this EventInformation info)
        => new(
            info.Guid,
            (Fargo.Api.Contracts.Events.EventType)(int)info.EventType,
            (Fargo.Api.Contracts.EntityType)(int)info.EntityType,
            info.EntityGuid,
            info.ActorGuid,
            info.OccurredAt);

    private static PermissionDto ToContract(this Permission permission)
        => new(permission.Guid, (Fargo.Api.Contracts.ActionType)(int)permission.Action);

    private static IReadOnlyCollection<UserPermissionUpdateModel>? ToUserPermissionModels(
        this IReadOnlyCollection<PermissionUpdateDto>? permissions)
        => permissions?.Select(static p => new UserPermissionUpdateModel((Fargo.Domain.ActionType)(int)p.Action)).ToArray();

    private static IReadOnlyCollection<UserGroupPermissionUpdateModel>? ToUserGroupPermissionModels(
        this IReadOnlyCollection<PermissionUpdateDto>? permissions)
        => permissions?.Select(static p => new UserGroupPermissionUpdateModel((Fargo.Domain.ActionType)(int)p.Action)).ToArray();

    private static Name? ToName(string? value) => value is null ? null : new Name(value);

    private static Description? ToDescription(string? value) => value is null ? null : new Description(value);

    private static FirstName? ToFirstName(string? value) => value is null ? null : new FirstName(value);

    private static LastName? ToLastName(string? value) => value is null ? null : new LastName(value);

    private static ArticleMetricsModel? ToApplicationModel(this ArticleMetricsDto? metrics)
        => metrics is null
            ? null
            : new ArticleMetricsModel(
                metrics.Mass.ToUnitsNet(),
                metrics.LengthX.ToUnitsNet(),
                metrics.LengthY.ToUnitsNet(),
                metrics.LengthZ.ToUnitsNet());

    private static Mass? ToUnitsNet(this MassDto? mass)
        => mass is null ? null : Mass.From(mass.Value, UnitParser.Default.Parse<MassUnit>(mass.Unit));

    private static Length? ToUnitsNet(this LengthDto? length)
        => length is null ? null : Length.From(length.Value, UnitParser.Default.Parse<LengthUnit>(length.Unit));

    private static ArticleMetricsDto? ToContract(this Fargo.Domain.Articles.ArticleMetrics? metrics)
        => metrics is null
            ? null
            : new ArticleMetricsDto(
                metrics.Mass.ToContract(),
                metrics.LengthX.ToContract(),
                metrics.LengthY.ToContract(),
                metrics.LengthZ.ToContract(),
                metrics.Density.ToContract());

    private static MassDto? ToContract(this Mass? mass)
        => mass is null ? null : new MassDto(mass.Value.Value, Mass.GetAbbreviation(mass.Value.Unit));

    private static LengthDto? ToContract(this Length? length)
        => length is null ? null : new LengthDto(length.Value.Value, Length.GetAbbreviation(length.Value.Unit));

    private static DensityDto? ToContract(this Density? density)
        => density is null ? null : new DensityDto(density.Value.Value, Density.GetAbbreviation(density.Value.Unit));

    private static ArticleImagesDto ToContract(this Fargo.Application.Articles.ArticleImages images)
        => new(images.HasImage);

    public static ArticleBarcodesDto ToContract(this Fargo.Domain.Articles.ArticleBarcodes barcodes)
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

    private static Fargo.Domain.Articles.ArticleBarcodes ToDomain(this ArticleBarcodesDto barcodes)
        => new()
        {
            Ean13 = barcodes.Ean13 is null ? null : new Ean13(barcodes.Ean13),
            Ean8 = barcodes.Ean8 is null ? null : new Ean8(barcodes.Ean8),
            UpcA = barcodes.UpcA is null ? null : new UpcA(barcodes.UpcA),
            UpcE = barcodes.UpcE is null ? null : new UpcE(barcodes.UpcE),
            Code128 = barcodes.Code128 is null ? null : new Code128(barcodes.Code128),
            Code39 = barcodes.Code39 is null ? null : new Code39(barcodes.Code39),
            Itf14 = barcodes.Itf14 is null ? null : new Itf14(barcodes.Itf14),
            Gs1128 = barcodes.Gs1128 is null ? null : new Gs1128(barcodes.Gs1128),
            QrCode = barcodes.QrCode is null ? null : new QrCode(barcodes.QrCode),
            DataMatrix = barcodes.DataMatrix is null ? null : new DataMatrix(barcodes.DataMatrix),
        };
}
