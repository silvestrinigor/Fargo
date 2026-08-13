using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Grpc.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoGrpcClient(this IServiceCollection services)
    {
        return services;
    }
}
