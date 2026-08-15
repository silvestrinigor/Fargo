using Fargo.Grpc.Client.Interceptors;
using Fargo.Grpc.V1;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Grpc.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoGrpcClient(this IServiceCollection services, Uri address)
    {
        services.AddTransient<AuthenticationInterceptor>();

        services.AddGrpcClient<IdentityService.IdentityServiceClient>(options =>
        {
            options.Address = address;
        });

        services.AddGrpcClient<ArticleService.ArticleServiceClient>(options =>
        {
            options.Address = address;
        });

        return services;
    }
}
