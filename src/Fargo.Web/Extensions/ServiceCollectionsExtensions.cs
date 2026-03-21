using Fargo.Infrastructure.Converters;

namespace Fargo.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureFargoWebHttpJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new NameJsonConverter());
            options.SerializerOptions.Converters.Add(new DescriptionJsonConverter());
            options.SerializerOptions.Converters.Add(new LimitJsonConverter());
            options.SerializerOptions.Converters.Add(new PageJsonConverter());
            options.SerializerOptions.Converters.Add(new NameidJsonConverter());
            options.SerializerOptions.Converters.Add(new PasswordJsonConverter());
            options.SerializerOptions.Converters.Add(new TokenJsonConverter());
            options.SerializerOptions.Converters.Add(new FirstNameJsonConverter());
            options.SerializerOptions.Converters.Add(new LastNameJsonConverter());
            options.SerializerOptions.Converters.Add(new NodeidJsonConverter());
        });

        return services;
    }
}