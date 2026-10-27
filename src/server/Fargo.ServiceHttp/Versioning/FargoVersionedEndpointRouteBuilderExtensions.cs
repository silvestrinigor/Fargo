using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Fargo.ServiceHttp.Versioning;

public static class FargoVersionedEndpointRouteBuilderExtensions
{
    /// <summary>
    /// The current API version. Version 1 is the baseline every endpoint starts
    /// at; a later version is introduced per endpoint only when that endpoint
    /// gets a breaking change (Model A, per-endpoint versioning).
    /// </summary>
    public static readonly ApiVersion V1 = new(1);

    /// <summary>
    /// Creates a versioned route group mounted under the <c>/v{version}</c>
    /// segment, declaring the versions the group's endpoints implement.
    /// </summary>
    /// <param name="builder">The root endpoint route builder.</param>
    /// <param name="apiName">
    /// The logical API name used to group the endpoints for the API explorer
    /// (for example "Articles").
    /// </param>
    /// <param name="path">
    /// The resource path appended after the version segment (for example
    /// "/articles"). A leading slash is optional.
    /// </param>
    /// <param name="versions">
    /// The versions the group's endpoints support. Defaults to <see cref="V1"/>.
    /// </param>
    /// <returns>A route group builder scoped to the declared version(s).</returns>
    public static RouteGroupBuilder MapVersionedGroup(
        this IEndpointRouteBuilder builder,
        string apiName,
        string path,
        params ApiVersion[] versions)
    {
        var effectiveVersions = versions.Length == 0 ? [V1] : versions;

        var versionSet = builder.NewApiVersionSet(apiName);

        foreach (var version in effectiveVersions)
        {
            versionSet.HasApiVersion(version);
        }

        var normalizedPath = path.StartsWith('/') ? path[1..] : path;

        var group = builder
            .MapGroup($"/v{{version:apiVersion}}/{normalizedPath}")
            .WithApiVersionSet(versionSet.Build());

        foreach (var version in effectiveVersions)
        {
            group.HasApiVersion(version);
        }

        return group;
    }
}
