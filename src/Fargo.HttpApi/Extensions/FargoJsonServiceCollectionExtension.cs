using Fargo.Infrastructure.Converters;

namespace Fargo.HttpApi.Extensions;

public static class FargoJsonServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureFargoJson()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new NameJsonConverter());
                options.SerializerOptions.Converters.Add(new PasswordJsonConverter());
                options.SerializerOptions.Converters.Add(new DescriptionJsonConverter());
                options.SerializerOptions.Converters.Add(new LimitJsonConverter());
                options.SerializerOptions.Converters.Add(new PageJsonConverter());
                options.SerializerOptions.Converters.Add(new TokenJsonConverter());
                options.SerializerOptions.Converters.Add(new FirstNameJsonConverter());
                options.SerializerOptions.Converters.Add(new LastNameJsonConverter());
                options.SerializerOptions.Converters.Add(new NameidJsonConverter());
                options.SerializerOptions.Converters.Add(new MassJsonConverter());
                options.SerializerOptions.Converters.Add(new LengthJsonConverter());
                options.SerializerOptions.Converters.Add(new DensityJsonConverter());
                options.SerializerOptions.Converters.Add(new ColorJsonConverter());
                options.SerializerOptions.Converters.Add(new Ean13JsonConverter());
                options.SerializerOptions.Converters.Add(new ScalarJsonConverter());
            });

            return services;
        }
    }
}
