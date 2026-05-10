using Fargo.GrpcApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.GrpcApi;

public static class FargoGrpcApiExtensions
{
    public static IServiceCollection AddFargoGrpcApi(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<FargoGrpcExceptionInterceptor>();
        });

        return services;
    }

    public static IEndpointRouteBuilder MapFargoGrpcApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<AuthenticationGrpcService>();
        endpoints.MapGrpcService<ArticleGrpcService>();
        endpoints.MapGrpcService<ItemGrpcService>();
        endpoints.MapGrpcService<PartitionGrpcService>();
        endpoints.MapGrpcService<UserGrpcService>();
        endpoints.MapGrpcService<UserGroupGrpcService>();

        return endpoints;
    }
}
