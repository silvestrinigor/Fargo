using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Informations;

namespace Fargo.Core.Tests.Audits;

public class AuditMetadataTests
{
    [Fact]
    public void Add_WhenNewProperty_ShouldStoreValue()
    {
        var metadata = new AuditMetadata();

        metadata.Add("key", new AuditValue.String("value"));

        Assert.True(metadata.Values.ContainsKey("key"));
        var value = Assert.IsType<AuditValue.String>(metadata.Values["key"]);
        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void Add_WhenDuplicateProperty_ShouldThrowInvalidOperation()
    {
        var metadata = new AuditMetadata();
        metadata.Add("key", new AuditValue.String("a"));

        Assert.Throws<InvalidOperationException>(
            () => metadata.Add("key", new AuditValue.String("b")));
    }

    [Fact]
    public void AddName_ShouldStoreStringUnderNameKey()
    {
        var metadata = new AuditMetadata();

        metadata.AddName(new Name("Widget"));

        var value = Assert.IsType<AuditValue.String>(metadata.Values["name"]);
        Assert.Equal("Widget", value.Value);
    }

    [Fact]
    public void AddIsActive_ShouldStoreBoolean()
    {
        var metadata = new AuditMetadata();

        metadata.AddIsActive(true);

        var value = Assert.IsType<AuditValue.Boolean>(metadata.Values["isActive"]);
        Assert.True(value.Value);
    }

    [Fact]
    public void AddShelfLife_WhenNull_ShouldStoreNull()
    {
        var metadata = new AuditMetadata();

        metadata.AddShelfLife(null);

        Assert.IsType<AuditValue.Null>(metadata.Values["shelfLife"]);
    }

    [Fact]
    public void AddShelfLife_WhenValue_ShouldStoreNumber()
    {
        var metadata = new AuditMetadata();

        metadata.AddShelfLife(TimeSpan.FromHours(1));

        var value = Assert.IsType<AuditValue.Number>(metadata.Values["shelfLife"]);
        Assert.Equal(TimeSpan.FromHours(1).Ticks, value.Value);
    }

    [Fact]
    public void AddParentContainer_WhenNull_ShouldStoreNull()
    {
        var metadata = new AuditMetadata();

        metadata.AddParentContainer(null);

        Assert.IsType<AuditValue.Null>(metadata.Values["parentContainerGuid"]);
    }

    [Fact]
    public void AddParentContainer_WhenValue_ShouldStoreStringGuid()
    {
        var metadata = new AuditMetadata();
        var guid = Guid.NewGuid();

        metadata.AddParentContainer(guid);

        var value = Assert.IsType<AuditValue.String>(metadata.Values["parentContainerGuid"]);
        Assert.Equal(guid.ToString(), value.Value);
    }

    [Fact]
    public void AddPermissions_ShouldStoreArrayOfNumbers()
    {
        var metadata = new AuditMetadata();

        metadata.AddPermissions([ActionType.CreateItem, ActionType.DeleteItem]);

        var array = Assert.IsType<AuditValue.Array>(metadata.Values["permissions"]);
        Assert.Equal(2, array.Values.Count);
        Assert.All(array.Values, v => Assert.IsType<AuditValue.Number>(v));
    }

    [Fact]
    public void AddPartitionsAdded_ShouldStoreArrayOfStringGuids()
    {
        var metadata = new AuditMetadata();
        var g1 = Guid.NewGuid();
        var g2 = Guid.NewGuid();

        metadata.AddPartitionsAdded([g1, g2]);

        var array = Assert.IsType<AuditValue.Array>(metadata.Values["partitionsAdded"]);
        Assert.Equal(2, array.Values.Count);
    }

    [Fact]
    public void AddPasswordChanged_ShouldStoreBooleanTrue()
    {
        var metadata = new AuditMetadata();

        metadata.AddPasswordChanged();

        var value = Assert.IsType<AuditValue.Boolean>(metadata.Values["passwordChanged"]);
        Assert.True(value.Value);
    }

    [Fact]
    public void AddRefreshTokensRevoked_ShouldStoreCount()
    {
        var metadata = new AuditMetadata();

        metadata.AddRefreshTokensRevoked(3);

        var value = Assert.IsType<AuditValue.Number>(metadata.Values["refreshTokensRevoked"]);
        Assert.Equal(3, value.Value);
    }

    [Fact]
    public void AddDimension_ShouldStoreObjectWithXYZ()
    {
        var metadata = new AuditMetadata();

        metadata.AddDimension(null, null, null);

        var obj = Assert.IsType<AuditValue.Object>(metadata.Values["dimension"]);
        Assert.True(obj.Value.ContainsKey("x"));
        Assert.True(obj.Value.ContainsKey("y"));
        Assert.True(obj.Value.ContainsKey("z"));
    }
}
