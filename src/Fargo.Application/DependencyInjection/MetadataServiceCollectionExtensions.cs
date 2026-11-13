using Fargo.Application.Dtos.Metadata;
using Fargo.Application.Queries.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class MetadataServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoMetadataApplication() => services
        .AddFargoQueryHandler<CatalogQuery, IReadOnlyList<CatalogEntryDto>, CatalogQueryHandler>();
    }
}
