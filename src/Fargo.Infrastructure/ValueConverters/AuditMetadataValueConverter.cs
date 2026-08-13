using Fargo.Core.Audits;
using Fargo.Infrastructure.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public sealed class AuditMetadataValueConverter : ValueConverter<AuditMetadata, string>
{
    public AuditMetadataValueConverter()
        : base(
            metadata => AuditMetadataJson.Serialize(metadata),
            json => AuditMetadataJson.Deserialize(json))
    {
    }
}
