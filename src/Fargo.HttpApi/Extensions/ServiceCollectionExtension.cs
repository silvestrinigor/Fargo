using Fargo.HttpApi.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring HTTP API services.
    /// </summary>
    public static class ServiceExtension
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
                    options.SerializerOptions.Converters.Add(new NameidJsonConverter());
                    options.SerializerOptions.Converters.Add(new PasswordJsonConverter());
                    options.SerializerOptions.Converters.Add(new TokenJsonConverter());
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
                            ValidIssuer = configuration.GetJwtConfiguration("Issuer"),
                            ValidAudience = configuration.GetJwtConfiguration("Audience"),
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration.GetJwtConfiguration("Key")!)
                            )
                        };
                    });

                services.AddAuthorization();

                return services;
            }
        }
    }
}