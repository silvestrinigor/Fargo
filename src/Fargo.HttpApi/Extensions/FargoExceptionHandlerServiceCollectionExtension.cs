using Fargo.HttpApi.ExceptionHandlers;

namespace Fargo.HttpApi.Extensions;

public static class FargoExceptionHandlerServiceCollectionExtension
{
    public static IServiceCollection AddFargoExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<FargoApplicationExceptionHandler>();

        services.AddExceptionHandler<FargoCoreExceptionHandler>();

        services.AddExceptionHandler<BadRequestExceptionHandler>();

        return services;
    }
}
