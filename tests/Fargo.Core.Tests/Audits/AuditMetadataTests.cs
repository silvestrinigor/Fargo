using Fargo.Core.Audits;

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
}
