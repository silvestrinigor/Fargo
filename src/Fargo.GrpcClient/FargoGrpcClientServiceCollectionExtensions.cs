using Fargo.GrpcContracts.Commands.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.GrpcClient;

public static class FargoGrpcClientServiceCollectionExtensions
{
    public static IServiceCollection AddFargoGrpcClient(
        this IServiceCollection services,
        Action<FargoGrpcClientOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var options = new FargoGrpcClientOptions();
        configure(options);

        if (options.Address is null)
        {
            throw new ArgumentException("Fargo gRPC client address is required.", nameof(configure));
        }

        services.TryAddSingleton(options);

        services
            .AddGrpcClient<WorkspaceService.WorkspaceServiceClient>(grpc =>
            {
                grpc.Address = options.Address;
            });

        services.Add(ServiceDescriptor.Describe(
            typeof(IFargoWorkspaceClient),
            typeof(FargoWorkspaceGrpcClient),
            lifetime));

        return services;
    }
}
