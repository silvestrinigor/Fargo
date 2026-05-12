using Fargo.Application;
using Fargo.Core;
using Fargo.Core.Barcodes;
using Google.Protobuf.WellKnownTypes;
using UnitsNet;
using UnitsNet.Units;
using AppArticles = Fargo.Application.Articles;
using AppItems = Fargo.Application.Items;
using AppPartitions = Fargo.Application.Partitions;
using AppUserGroups = Fargo.Application.UserGroups;
using AppUsers = Fargo.Application.Users;
using Contracts = Fargo.GrpcContracts;
using DomainActionType = Fargo.Core.ActionType;
using DomainBarcodeFormat = Fargo.Core.Barcodes.BarcodeFormat;

namespace Fargo.GrpcApi;

internal static class GrpcContractMappings
{
    public static Pagination ToPagination(this Contracts.GetManyRequest request)
        => new(
            request.HasPage ? new Page(request.Page) : Page.FirstPage,
            request.HasLimit ? new Limit(request.Limit) : Limit.MaxLimit);

    public static DateTimeOffset? ToDateTimeOffset(this Timestamp? timestamp)
        => timestamp?.ToDateTimeOffset();

    public static TimeSpan? ToTimeSpan(this Google.Protobuf.WellKnownTypes.Duration? duration)
        => duration?.ToTimeSpan();

    public static Guid ToGuid(this string value, string parameterName)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new ArgumentException($"'{value}' is not a valid GUID.", parameterName);
    }

    public static IReadOnlyCollection<Guid>? ToGuidCollectionOrNull(this IEnumerable<string> values)
    {
        var guids = values.Select(static value => value.ToGuid(nameof(values))).ToArray();
        return guids.Length == 0 ? null : guids;
    }

    public static IReadOnlyCollection<Guid> ToGuidCollection(this IEnumerable<string> values)
        => values.Select(static value => value.ToGuid(nameof(values))).ToArray();

    public static Contracts.GuidResult ToGuidResult(this Guid guid)
        => new() { Guid = guid.ToString() };

    public static Contracts.AuthInfo ToInfo(this Application.Authentication.AuthResult result)
    {
        var response = new Contracts.AuthInfo
        {
            AccessToken = result.AccessToken.Value,
            RefreshToken = result.RefreshToken.Value,
            ExpiresAt = Timestamp.FromDateTimeOffset(result.ExpiresAt),
            IsAdmin = result.IsAdmin
        };

        response.PermissionActions.AddRange(result.PermissionActions.Select(static action => (Contracts.ActionType)(int)action));
        response.PartitionAccesses.AddRange(result.PartitionAccesses.Select(static guid => guid.ToString()));

        return response;
    }

    public static Contracts.ArticleInfo ToInfo(this AppArticles.ArticleDto article)
    {
        var response = new Contracts.ArticleInfo
        {
            Guid = article.Guid.ToString(),
            Name = article.Name.Value,
            Description = article.Description.Value,
            Metrics = article.Metrics.ToInfo(),
            Barcodes = article.Barcodes.ToInfo(),
            IsActive = article.IsActive
        };

        if (article.ShelfLife is { } shelfLife)
        {
            response.ShelfLife = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(shelfLife);
        }

        if (article.EditedByGuid is { } editedByGuid)
        {
            response.EditedByGuid = editedByGuid.ToString();
        }

        response.Partitions.AddRange(article.Partitions.Select(static guid => guid.ToString()));

        return response;
    }

    public static AppArticles.ArticleCreateDto ToApplicationDto(this Contracts.ArticleCreateRequest request)
        => new(
            new Name(request.Name),
            request.HasDescription ? new Description(request.Description) : null,
            request.ShelfLife.ToTimeSpan(),
            request.Metrics?.ToApplicationDto(),
            request.Barcodes?.ToApplicationDto(),
            request.Partitions.ToGuidCollectionOrNull(),
            request.HasIsActive ? request.IsActive : null);

    public static AppArticles.ArticleUpdateDto ToApplicationDto(this Contracts.ArticleUpdateRequest request)
        => new(
            new Name(request.Name),
            new Description(request.Description),
            request.ShelfLife.ToTimeSpan(),
            request.Metrics?.ToApplicationDto() ?? new AppArticles.ArticleMetricsDto(),
            request.Barcodes?.ToApplicationDto() ?? new AppArticles.ArticleBarcodesDto(),
            request.Partitions.ToGuidCollection(),
            request.IsActive);

    public static AppArticles.ArticleBarcodeDto ToApplicationDto(this Contracts.ArticleBarcode barcode)
        => new(barcode.Barcode, (DomainBarcodeFormat)(int)barcode.Type);

    public static Contracts.ItemInfo ToInfo(this AppItems.ItemDto item)
    {
        var response = new Contracts.ItemInfo
        {
            Guid = item.Guid.ToString(),
            ArticleGuid = item.ArticleGuid.ToString()
        };

        if (item.ProductionDate is { } productionDate)
        {
            response.ProductionDate = Timestamp.FromDateTimeOffset(productionDate);
        }

        if (item.ParentContainerGuid is { } parentContainerGuid)
        {
            response.ParentContainerGuid = parentContainerGuid.ToString();
        }

        if (item.EditedByGuid is { } editedByGuid)
        {
            response.EditedByGuid = editedByGuid.ToString();
        }

        response.Partitions.AddRange(item.Partitions.Select(static guid => guid.ToString()));

        return response;
    }

    public static AppItems.ItemCreateDto ToApplicationDto(this Contracts.ItemCreateRequest request)
        => new(
            request.ArticleGuid.ToGuid(nameof(request.ArticleGuid)),
            request.ProductionDate.ToDateTimeOffset(),
            request.Partitions.ToGuidCollectionOrNull());

    public static AppItems.ItemUpdateDto ToApplicationDto(this Contracts.ItemUpdateRequest request)
        => new(
            request.Partitions.ToGuidCollection(),
            request.HasParentContainerGuid ? request.ParentContainerGuid.ToGuid(nameof(request.ParentContainerGuid)) : null);

    public static Contracts.PartitionInfo ToInfo(this AppPartitions.PartitionDto partition)
    {
        var response = new Contracts.PartitionInfo
        {
            Guid = partition.Guid.ToString(),
            Name = partition.Name.Value,
            Description = partition.Description.Value,
            IsActive = partition.IsActive
        };

        if (partition.ParentPartitionGuid is { } parentPartitionGuid)
        {
            response.ParentPartitionGuid = parentPartitionGuid.ToString();
        }

        if (partition.EditedByGuid is { } editedByGuid)
        {
            response.EditedByGuid = editedByGuid.ToString();
        }

        return response;
    }

    public static AppPartitions.PartitionCreateDto ToApplicationDto(this Contracts.PartitionCreateRequest request)
        => new(
            new Name(request.Name),
            request.HasDescription ? new Description(request.Description) : null,
            request.HasParentPartitionGuid ? request.ParentPartitionGuid.ToGuid(nameof(request.ParentPartitionGuid)) : null);

    public static AppPartitions.PartitionUpdateDto ToApplicationDto(this Contracts.PartitionUpdateRequest request)
        => new(
            request.HasName ? new Name(request.Name) : null,
            request.HasDescription ? new Description(request.Description) : null,
            request.HasParentPartitionGuid ? request.ParentPartitionGuid.ToGuid(nameof(request.ParentPartitionGuid)) : null,
            request.HasIsActive ? request.IsActive : null);

    public static Contracts.UserInfo ToInfo(this AppUsers.UserDto user)
    {
        var response = new Contracts.UserInfo
        {
            Guid = user.Guid.ToString(),
            Nameid = user.Nameid.Value,
            Description = user.Description.Value,
            IsActive = user.IsActive
        };

        if (user.FirstName is { } firstName)
        {
            response.FirstName = firstName.Value;
        }

        if (user.LastName is { } lastName)
        {
            response.LastName = lastName.Value;
        }

        if (user.DefaultPasswordExpirationPeriod is { } defaultPasswordExpirationPeriod)
        {
            response.DefaultPasswordExpirationPeriod = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(defaultPasswordExpirationPeriod);
        }

        if (user.RequirePasswordChangeAt is { } requirePasswordChangeAt)
        {
            response.RequirePasswordChangeAt = Timestamp.FromDateTimeOffset(requirePasswordChangeAt);
        }

        if (user.EditedByGuid is { } editedByGuid)
        {
            response.EditedByGuid = editedByGuid.ToString();
        }

        response.Permissions.AddRange(user.Permissions.Select(static permission => new Contracts.PermissionInfo
        {
            Guid = permission.Guid.ToString(),
            Action = (Contracts.ActionType)(int)permission.Action
        }));
        response.Partitions.AddRange(user.Partitions.Select(static guid => guid.ToString()));
        response.UserGroups.AddRange(user.UserGroups.Select(static guid => guid.ToString()));

        return response;
    }

    public static AppUsers.UserCreateDto ToApplicationDto(this Contracts.UserCreateRequest request)
        => new(
            request.Nameid,
            request.Password,
            request.HasFirstName ? new Core.Users.FirstName(request.FirstName) : null,
            request.HasLastName ? new Core.Users.LastName(request.LastName) : null,
            request.HasDescription ? new Description(request.Description) : null,
            request.Permissions.Select(static permission => new AppUsers.UserPermissionUpdateDto((DomainActionType)(int)permission.Action)).ToArray(),
            request.DefaultPasswordExpirationPeriod.ToTimeSpan(),
            request.Partitions.ToGuidCollectionOrNull(),
            request.UserGroups.ToGuidCollectionOrNull());

    public static AppUsers.UserUpdateDto ToApplicationDto(this Contracts.UserUpdateRequest request)
        => new(
            request.HasNameid ? request.Nameid : null,
            request.HasFirstName ? new Core.Users.FirstName(request.FirstName) : null,
            request.HasLastName ? new Core.Users.LastName(request.LastName) : null,
            request.HasDescription ? new Description(request.Description) : null,
            request.HasPassword ? request.Password : null,
            request.HasIsActive ? request.IsActive : null,
            request.PermissionsSet
                ? request.Permissions.Select(static permission => new AppUsers.UserPermissionUpdateDto((DomainActionType)(int)permission.Action)).ToArray()
                : null,
            request.DefaultPasswordExpirationPeriod.ToTimeSpan(),
            request.PartitionsSet ? request.Partitions.ToGuidCollection() : null,
            request.UserGroupsSet ? request.UserGroups.ToGuidCollection() : null);

    public static Contracts.UserGroupInfo ToInfo(this AppUserGroups.UserGroupDto userGroup)
    {
        var response = new Contracts.UserGroupInfo
        {
            Guid = userGroup.Guid.ToString(),
            Nameid = userGroup.Nameid.Value,
            Description = userGroup.Description.Value,
            IsActive = userGroup.IsActive
        };

        if (userGroup.EditedByGuid is { } editedByGuid)
        {
            response.EditedByGuid = editedByGuid.ToString();
        }

        response.Permissions.AddRange(userGroup.Permissions.Select(static permission => new Contracts.PermissionInfo
        {
            Guid = permission.Guid.ToString(),
            Action = (Contracts.ActionType)(int)permission.Action
        }));
        response.Partitions.AddRange(userGroup.Partitions.Select(static guid => guid.ToString()));

        return response;
    }

    public static AppUserGroups.UserGroupCreateDto ToApplicationDto(this Contracts.UserGroupCreateRequest request)
        => new(
            request.Nameid,
            request.HasDescription ? new Description(request.Description) : null,
            request.Permissions.Select(static permission => new AppUserGroups.UserGroupPermissionUpdateDto((DomainActionType)(int)permission.Action)).ToArray(),
            request.Partitions.ToGuidCollectionOrNull());

    public static AppUserGroups.UserGroupUpdateDto ToApplicationDto(this Contracts.UserGroupUpdateRequest request)
        => new(
            request.HasNameid ? request.Nameid : null,
            request.HasDescription ? new Description(request.Description) : null,
            request.HasIsActive ? request.IsActive : null,
            request.PermissionsSet
                ? request.Permissions.Select(static permission => new AppUserGroups.UserGroupPermissionUpdateDto((DomainActionType)(int)permission.Action)).ToArray()
                : null,
            request.PartitionsSet ? request.Partitions.ToGuidCollection() : null);

    private static Contracts.ArticleMetricsInfo ToInfo(this AppArticles.ArticleMetricsDto metrics)
    {
        var response = new Contracts.ArticleMetricsInfo();

        if (metrics.Mass is { } mass)
        {
            response.Mass = new Contracts.MassInfo { Value = mass.Value, Unit = Mass.GetAbbreviation(mass.Unit) };
        }

        if (metrics.LengthX is { } lengthX)
        {
            response.LengthX = new Contracts.LengthInfo { Value = lengthX.Value, Unit = Length.GetAbbreviation(lengthX.Unit) };
        }

        if (metrics.LengthY is { } lengthY)
        {
            response.LengthY = new Contracts.LengthInfo { Value = lengthY.Value, Unit = Length.GetAbbreviation(lengthY.Unit) };
        }

        if (metrics.LengthZ is { } lengthZ)
        {
            response.LengthZ = new Contracts.LengthInfo { Value = lengthZ.Value, Unit = Length.GetAbbreviation(lengthZ.Unit) };
        }

        return response;
    }

    private static AppArticles.ArticleMetricsDto ToApplicationDto(this Contracts.ArticleMetricsInfo metrics)
        => new(
            metrics.Mass is null ? null : Mass.From(metrics.Mass.Value, UnitParser.Default.Parse<MassUnit>(metrics.Mass.Unit)),
            metrics.LengthX is null ? null : Length.From(metrics.LengthX.Value, UnitParser.Default.Parse<LengthUnit>(metrics.LengthX.Unit)),
            metrics.LengthY is null ? null : Length.From(metrics.LengthY.Value, UnitParser.Default.Parse<LengthUnit>(metrics.LengthY.Unit)),
            metrics.LengthZ is null ? null : Length.From(metrics.LengthZ.Value, UnitParser.Default.Parse<LengthUnit>(metrics.LengthZ.Unit)));

    private static Contracts.ArticleBarcodesInfo ToInfo(this AppArticles.ArticleBarcodesDto barcodes)
    {
        var response = new Contracts.ArticleBarcodesInfo();

        if (barcodes.Ean13 is { } ean13)
        {
            response.Ean13 = ean13.Code;
        }

        if (barcodes.Ean8 is { } ean8)
        {
            response.Ean8 = ean8.Code;
        }

        if (barcodes.UpcA is { } upcA)
        {
            response.UpcA = upcA.Code;
        }

        if (barcodes.UpcE is { } upcE)
        {
            response.UpcE = upcE.Code;
        }

        if (barcodes.Code128 is { } code128)
        {
            response.Code128 = code128.Code;
        }

        if (barcodes.Code39 is { } code39)
        {
            response.Code39 = code39.Code;
        }

        if (barcodes.Itf14 is { } itf14)
        {
            response.Itf14 = itf14.Code;
        }

        if (barcodes.Gs1128 is { } gs1128)
        {
            response.Gs1128 = gs1128.Code;
        }

        if (barcodes.QrCode is { } qrCode)
        {
            response.QrCode = qrCode.Code;
        }

        if (barcodes.DataMatrix is { } dataMatrix)
        {
            response.DataMatrix = dataMatrix.Code;
        }

        return response;
    }

    private static AppArticles.ArticleBarcodesDto ToApplicationDto(this Contracts.ArticleBarcodesInfo barcodes)
        => new(
            barcodes.HasEan13 ? new Ean13(barcodes.Ean13) : null,
            barcodes.HasEan8 ? new Ean8(barcodes.Ean8) : null,
            barcodes.HasUpcA ? new UpcA(barcodes.UpcA) : null,
            barcodes.HasUpcE ? new UpcE(barcodes.UpcE) : null,
            barcodes.HasCode128 ? new Code128(barcodes.Code128) : null,
            barcodes.HasCode39 ? new Code39(barcodes.Code39) : null,
            barcodes.HasItf14 ? new Itf14(barcodes.Itf14) : null,
            barcodes.HasGs1128 ? new Gs1128(barcodes.Gs1128) : null,
            barcodes.HasQrCode ? new QrCode(barcodes.QrCode) : null,
            barcodes.HasDataMatrix ? new DataMatrix(barcodes.DataMatrix) : null);
}
