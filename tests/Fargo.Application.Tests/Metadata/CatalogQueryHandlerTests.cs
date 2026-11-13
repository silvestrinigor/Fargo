using Fargo.Application.Queries.Metadata;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Tests.Metadata;

public class CatalogQueryHandlerTests
{
    private readonly CatalogQueryHandler handler = new();

    [Fact]
    public async Task EntityTypes_ReturnsEveryEnumMemberWithItsCode()
    {
        var result = await handler.HandleAsync(new CatalogQuery(MetadataCatalog.EntityTypes));

        Assert.Equal(Enum.GetValues<EntityType>().Length, result.Count);
        Assert.Contains(result, e => e.Id == (int)EntityType.SupplyOrder && e.Name == nameof(EntityType.SupplyOrder));
    }

    [Fact]
    public async Task AuditLogTypes_ReturnsEveryEnumMember()
    {
        var result = await handler.HandleAsync(new CatalogQuery(MetadataCatalog.AuditLogTypes));

        Assert.Equal(
            Enum.GetValues<AuditLogType>().Select(v => (int)v).Order(),
            result.Select(e => e.Id));
    }

    [Theory]
    [MemberData(nameof(AllCatalogs))]
    public async Task EveryCatalog_IsNotEmptyAndHasUniqueIds(MetadataCatalog catalog)
    {
        var result = await handler.HandleAsync(new CatalogQuery(catalog));

        Assert.NotEmpty(result);
        Assert.Equal(result.Count, result.Select(e => e.Id).Distinct().Count());
    }

    [Fact]
    public async Task ActionTypes_ContainsTheLegacyEditItemPermission()
    {
        var result = await handler.HandleAsync(new CatalogQuery(MetadataCatalog.ActionTypes));

        Assert.Contains(result, e => e.Id == (int)ActionType.EditItem);
    }

    public static TheoryData<MetadataCatalog> AllCatalogs() => new(Enum.GetValues<MetadataCatalog>());
}
