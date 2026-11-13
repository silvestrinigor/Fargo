using Asp.Versioning;

namespace Fargo.Api.Versioning;

public static class FargoVersionedEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Version 1 is the baseline every endpoint starts at; a later version is introduced per endpoint only when
    /// that endpoint gets a breaking change.
    /// </summary>
    public static readonly ApiVersion V1 = new(1);

    /// <summary>
    /// Creates a route group under the <c>/v{version}</c> segment, declaring the versions its endpoints implement
    /// (default <see cref="V1"/>). <paramref name="apiName"/> groups the endpoints in the API explorer.
    /// </summary>
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
