namespace Fargo.HttpApi.ExceptionHandlers;

public static class FargoExceptionHandlerExtension
{
    public static IServiceCollection AddFargoExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<FargoApplicationExceptionHandler>();

        services.AddExceptionHandler<FargoCoreExceptionHandler>();

        return services;
    }
}
