namespace Fargo.Http.ExceptionHandlers;

public static class FargoExceptionHandlerServiceCollectionExtensions
{
    public static IServiceCollection AddFargoExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<FargoCoreExceptionHandler>();

        services.AddExceptionHandler<FargoApplicationExceptionHandler>();

        services.AddExceptionHandler<FargoInfrastructureExceptionHandler>();

        services.AddExceptionHandler<BadRequestExceptionHandler>();

        return services;
    }
}
