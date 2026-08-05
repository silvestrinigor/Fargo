namespace Fargo.AppHost.Extensions;

public static class FargoResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> WithFargoEnvironment(
        this IResourceBuilder<ProjectResource> resource,
        string environmentName)
    {
        return resource
        .WithEnvironment(
            "ASPNETCORE_ENVIRONMENT",
            environmentName)
        .WithEnvironment(
            "DOTNET_ENVIRONMENT",
            environmentName);
    }
}
