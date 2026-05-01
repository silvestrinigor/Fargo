using Fargo.Application.ApiClients;
using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.Tree;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Users;
using Fargo.Sdk.Contracts.ApiClients;
using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Contracts.Authentication;
using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Permissions;
using Fargo.Sdk.Contracts.Tree;
using Fargo.Sdk.Contracts.UserGroups;
using Fargo.Sdk.Contracts.Users;
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
            result.PermissionActions.Select(static action => (Fargo.Sdk.Contracts.ActionType)(int)action).ToArray(),
            result.PartitionAccesses.ToArray());

    public static ApiClientCreatedDto ToContract(this ApiClientCreatedResult result)
        => new(result.Guid, result.PlainKey);

    public static LoginCommand ToCommand(this LoginRequest request)
        => new(request.Nameid, request.Password);

    public static LogoutCommand ToCommand(this LogoutRequest request)
        => new(new Fargo.Domain.Tokens.Token(request.RefreshToken));

    public static RefreshCommand ToCommand(this RefreshRequest request)
        => new(new Fargo.Domain.Tokens.Token(request.RefreshToken));

    public static PasswordChangeCommand ToCommand(this PasswordChangeRequest request)
        => new(new UserPasswordUpdateModel(request.Passwords.NewPassword, request.Passwords.CurrentPassword));

    public static ApiClientCreateCommand ToCommand(this ApiClientCreateRequest request)
        => new(request.Name, request.Description);

    public static ApiClientUpdateCommand ToCommand(this ApiClientUpdateRequest request, Guid guid)
        => new(guid, request.Name, request.Description, request.IsActive);

    public static ApiClientDto ToContract(this ApiClientInformation info)
        => new(info.Guid, info.Name, info.Description, info.IsActive, info.EditedByGuid);

    public static ArticleCreateCommand ToCommand(this ArticleCreateRequest request)
        => new(new ArticleCreateModel(
            new Name(request.Article.Name),
            ToDescription(request.Article.Description),
            request.Article.FirstPartition,
            request.Article.Metrics.ToApplicationModel(),
            request.Article.ShelfLife));

    public static ArticleUpdateCommand ToCommand(this ArticleUpdateRequest request, Guid articleGuid)
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

    public static ItemCreateCommand ToCommand(this ItemCreateRequest request)
        => new(new ItemCreateModel(
            request.Item.ArticleGuid,
            request.Item.FirstPartition,
            request.Item.ProductionDate));

    public static ItemUpdateCommand ToCommand(this ItemUpdateRequest request, Guid itemGuid)
        => new(itemGuid, new ItemUpdateModel(request.ProductionDate));

    public static ItemDto ToContract(this ItemInformation info)
        => new(info.Guid, info.ArticleGuid, info.ProductionDate, info.EditedByGuid);

    public static PartitionCreateCommand ToCommand(this PartitionCreateRequest request)
        => new(new Name(request.Name), ToDescription(request.Description), request.ParentPartitionGuid);

    public static PartitionUpdateCommand ToCommand(this PartitionUpdateRequest request, Guid partitionGuid)
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

    public static UserCreateCommand ToCommand(this UserCreateRequest request)
        => new(new UserCreateModel(
            request.User.Nameid,
            request.User.Password,
            ToFirstName(request.User.FirstName),
            ToLastName(request.User.LastName),
            ToDescription(request.User.Description),
            request.User.Permissions.ToUserPermissionModels(),
            request.User.DefaultPasswordExpirationTimeSpan,
            request.User.FirstPartition));

    public static UserUpdateCommand ToCommand(this UserUpdateRequest request, Guid userGuid)
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

    public static UserGroupCreateCommand ToCommand(this UserGroupCreateRequest request)
        => new(new UserGroupCreateModel(
            request.UserGroup.Nameid,
            ToDescription(request.UserGroup.Description),
            request.UserGroup.Permissions.ToUserGroupPermissionModels(),
            request.UserGroup.FirstPartition));

    public static UserGroupUpdateCommand ToCommand(this UserGroupUpdateRequest request, Guid userGroupGuid)
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

    public static Fargo.Sdk.Contracts.Events.EventDto ToContract(this EventInformation info)
        => new(
            info.Guid,
            (Fargo.Sdk.Contracts.Events.EventType)(int)info.EventType,
            (Fargo.Sdk.Contracts.Events.EntityType)(int)info.EntityType,
            info.EntityGuid,
            info.ActorGuid,
            info.OccurredAt);

    public static EntityTreeNodeDto ToContract(this EntityTreeNode node)
        => new(
            new NodeIdDto((Fargo.Sdk.Contracts.Tree.TreeNodeType)(int)node.TreeNodeType, node.EntityGuid),
            node.Title,
            node.Subtitle,
            node.HasChildren,
            node.IsActive);

    private static PermissionDto ToContract(this Permission permission)
        => new(permission.Guid, (Fargo.Sdk.Contracts.ActionType)(int)permission.Action);

    private static IReadOnlyCollection<UserPermissionUpdateModel>? ToUserPermissionModels(
        this IReadOnlyCollection<PermissionUpdateRequest>? permissions)
        => permissions?.Select(static p => new UserPermissionUpdateModel((ActionType)(int)p.Action)).ToArray();

    private static IReadOnlyCollection<UserGroupPermissionUpdateModel>? ToUserGroupPermissionModels(
        this IReadOnlyCollection<PermissionUpdateRequest>? permissions)
        => permissions?.Select(static p => new UserGroupPermissionUpdateModel((ActionType)(int)p.Action)).ToArray();

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
            barcodes.Ean13.ToContract(),
            barcodes.Ean8.ToContract(),
            barcodes.UpcA.ToContract(),
            barcodes.UpcE.ToContract(),
            barcodes.Code128.ToContract(),
            barcodes.Code39.ToContract(),
            barcodes.Itf14.ToContract(),
            barcodes.Gs1128.ToContract(),
            barcodes.QrCode.ToContract(),
            barcodes.DataMatrix.ToContract());

    private static BarcodeValueDto? ToContract(this Ean13? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this Ean8? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this UpcA? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this UpcE? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this Code128? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this Code39? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this Itf14? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this Gs1128? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this QrCode? value) => value is null ? null : new(value.Value.Code);
    private static BarcodeValueDto? ToContract(this DataMatrix? value) => value is null ? null : new(value.Value.Code);

    private static Fargo.Domain.Articles.ArticleBarcodes ToDomain(this ArticleBarcodesDto barcodes)
        => new()
        {
            Ean13 = barcodes.Ean13 is null ? null : new Ean13(barcodes.Ean13.Code),
            Ean8 = barcodes.Ean8 is null ? null : new Ean8(barcodes.Ean8.Code),
            UpcA = barcodes.UpcA is null ? null : new UpcA(barcodes.UpcA.Code),
            UpcE = barcodes.UpcE is null ? null : new UpcE(barcodes.UpcE.Code),
            Code128 = barcodes.Code128 is null ? null : new Code128(barcodes.Code128.Code),
            Code39 = barcodes.Code39 is null ? null : new Code39(barcodes.Code39.Code),
            Itf14 = barcodes.Itf14 is null ? null : new Itf14(barcodes.Itf14.Code),
            Gs1128 = barcodes.Gs1128 is null ? null : new Gs1128(barcodes.Gs1128.Code),
            QrCode = barcodes.QrCode is null ? null : new QrCode(barcodes.QrCode.Code),
            DataMatrix = barcodes.DataMatrix is null ? null : new DataMatrix(barcodes.DataMatrix.Code),
        };
}
