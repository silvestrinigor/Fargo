using Fargo.HttpApi.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fargo.HttpApi.Extensions
{
    public static class ServiceExtension
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
                    options.SerializerOptions.Converters.Add(new NameidJsonConverter());
                    options.SerializerOptions.Converters.Add(new PasswordJsonConverter());
                });

                return services;
            }

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