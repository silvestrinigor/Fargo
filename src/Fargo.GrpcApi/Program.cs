using Fargo.GrpcApi;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Security;
using Fargo.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();
builder.Services.AddFargoInfrastructure(builder.Configuration);
builder.Services.AddFargoGrpcAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddFargoGrpcApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapFargoGrpcApi();
app.MapDefaultEndpoints();

app.MapGet("/", () => "Fargo gRPC host is running. Use a gRPC client to call the services.");

app.Run();

internal static class FargoGrpcAuthentication
{
    public static IServiceCollection AddFargoGrpcAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };

                options.UseFargoTokenValidation();
            });

        return services;
    }
}
