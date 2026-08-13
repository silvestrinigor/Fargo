using Fargo.Http.ExceptionHandlers;

namespace Fargo.Http.Extensions;

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
