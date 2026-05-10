using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public static class FargoGrpcClientServiceCollectionExtensions
{
    public static IServiceCollection AddFargoGrpcClient(
        this IServiceCollection services,
        Action<FargoGrpcClientOptions> configure)
    {
        var options = new FargoGrpcClientOptions();
        configure(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton(sp =>
        {
            var configuredOptions = sp.GetRequiredService<FargoGrpcClientOptions>();
            return GrpcChannel.ForAddress(
                configuredOptions.Server
                ?? throw new InvalidOperationException("FargoGrpcClientOptions.Server is required."));
        });

        services.TryAddSingleton<FargoGrpcCallExecutor>();

        services.TryAddSingleton(sp => new Contracts.AuthenticationGrpc.AuthenticationGrpcClient(sp.GetRequiredService<GrpcChannel>()));
        services.TryAddSingleton(sp => new Contracts.ArticlesGrpc.ArticlesGrpcClient(sp.GetRequiredService<GrpcChannel>()));
        services.TryAddSingleton(sp => new Contracts.ItemsGrpc.ItemsGrpcClient(sp.GetRequiredService<GrpcChannel>()));
        services.TryAddSingleton(sp => new Contracts.PartitionsGrpc.PartitionsGrpcClient(sp.GetRequiredService<GrpcChannel>()));
        services.TryAddSingleton(sp => new Contracts.UsersGrpc.UsersGrpcClient(sp.GetRequiredService<GrpcChannel>()));
        services.TryAddSingleton(sp => new Contracts.UserGroupsGrpc.UserGroupsGrpcClient(sp.GetRequiredService<GrpcChannel>()));

        services.TryAddSingleton<FargoAuthenticationGrpcClient>();
        services.TryAddSingleton<FargoArticleGrpcClient>();
        services.TryAddSingleton<FargoItemGrpcClient>();
        services.TryAddSingleton<FargoPartitionGrpcClient>();
        services.TryAddSingleton<FargoUserGrpcClient>();
        services.TryAddSingleton<FargoUserGroupGrpcClient>();
        services.TryAddSingleton<FargoGrpcClient>();

        return services;
    }
}
