using Asp.Versioning;
using Fargo.ServiceHttp.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace Fargo.ServiceHttp.Versioning;

public static class FargoApiVersioningServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// URL-segment API versioning with per-endpoint version sets: each endpoint advertises only the versions it
        /// implements, so a breaking change to one endpoint introduces a new version for that endpoint alone.
        /// </summary>
        /// <remarks>
        /// The version is a required <c>v{version}</c> segment at the start of the route (for example
        /// <c>/v1/articles</c>); the default is <c>1.0</c>. The API explorer groups documents as <c>v1</c>, <c>v2</c>,
        /// which the built-in OpenAPI pipeline renders as one document per version, with Fargo's schema
        /// transformers applied to each.
        /// </remarks>
        public IServiceCollection AddFargoApiVersioning()
        {
            services
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1);
                    options.AssumeDefaultVersionWhenUnspecified = false;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(options =>
                {
                    // Document names become v1, v2, … (major-only), matching the
                    // v{version} route segment.
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                })
                .AddOpenApi();

            // Configure each per-version OpenAPI document with Fargo's schema
            // transformers. One registration per published version document name.
            foreach (var documentName in VersionedDocumentNames)
            {
                services.AddOpenApi(documentName, ConfigureFargoOpenApi);
            }

            return services;
        }
    }

    private static readonly string[] VersionedDocumentNames = ["v1"];

    private static void ConfigureFargoOpenApi(OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, context, _) =>
        {
            FargoOpenApiSchemaTransformers.ApplyFargoParameters(schema, context.ParameterDescription?.Type);

            FargoOpenApiSchemaTransformers.ApplyFargoTypes(schema, context.JsonTypeInfo.Type);

            return Task.CompletedTask;
        });
    }
}

