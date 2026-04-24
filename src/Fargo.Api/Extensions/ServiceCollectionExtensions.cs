using Fargo.Domain;
using Fargo.Infrastructure.Converters;
using Fargo.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Nodes;

namespace Fargo.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring HTTP API services.
/// </summary>
public static class ServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures the JSON serialization options used by the HTTP API.
        ///
        /// Custom converters are registered to support application-specific
        /// value objects and strongly typed models during JSON serialization
        /// and deserialization.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
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
                options.SerializerOptions.Converters.Add(new NodeidJsonConverter());
                options.SerializerOptions.Converters.Add(new MassJsonConverter());
                options.SerializerOptions.Converters.Add(new LengthJsonConverter());
            });

            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication and authorization services.
        ///
        /// The token validation setup ensures that the issuer, audience,
        /// lifetime, and signing key are validated for all incoming JWTs.
        /// </summary>
        /// <param name="configuration">
        /// The application configuration used to retrieve JWT settings such as
        /// issuer, audience, and signing key.
        /// </param>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
        public IServiceCollection AddFargoAuthentication(IConfiguration configuration)
        {
            var jwt = configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt configuration is missing.");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwt.Key))
                    };
                });

            return services;
        }

        /// <summary>
        /// Adds OpenAPI configuration including schema transformations
        /// for custom pagination value objects such as <see cref="Page"/> and <see cref="Limit"/>.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
        public IServiceCollection AddFargoOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, context, _) =>
                {
                    if (context.ParameterDescription?.Type == typeof(Page?))
                    {
                        schema.Type = Microsoft.OpenApi.JsonSchemaType.Integer;
                        schema.Minimum = Page.MinValue.ToString();
                        schema.Default = JsonValue.Create(Page.FirstPage.Value);
                    }

                    if (context.ParameterDescription?.Type == typeof(Limit?))
                    {
                        schema.Type = Microsoft.OpenApi.JsonSchemaType.Integer;
                        schema.Minimum = Limit.MinValue.ToString();
                        schema.Maximum = Limit.MaxValue.ToString();
                        schema.Default = JsonValue.Create(Limit.MaxLimit.Value);
                    }

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
