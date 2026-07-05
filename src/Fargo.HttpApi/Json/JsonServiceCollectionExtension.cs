using Fargo.Infrastructure.Converters;

namespace Fargo.HttpApi.Json;

public static class JsonServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureFargoHttpJsonOptions()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new NameJsonConverter());
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
                options.SerializerOptions.Converters.Add(new Ean8JsonConverter());
                options.SerializerOptions.Converters.Add(new UpcAJsonConverter());
                options.SerializerOptions.Converters.Add(new UpcEJsonConverter());
                options.SerializerOptions.Converters.Add(new Code128JsonConverter());
                options.SerializerOptions.Converters.Add(new Code39JsonConverter());
                options.SerializerOptions.Converters.Add(new Itf14JsonConverter());
                options.SerializerOptions.Converters.Add(new Gs1128JsonConverter());
                options.SerializerOptions.Converters.Add(new QrCodeJsonConverter());
                options.SerializerOptions.Converters.Add(new DataMatrixJsonConverter());
            });

            return services;
        }
    }
}
