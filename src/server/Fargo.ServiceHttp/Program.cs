using Fargo.Application.Extensions;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Fargo.ServiceDefaults;
using Fargo.ServiceHttp.Endpoints;
using Fargo.ServiceHttp.ExceptionHandlers;
using Fargo.ServiceHttp.Hosting;
using Fargo.ServiceHttp.JsonConverters;
using Fargo.ServiceHttp.OpenApi;
using Fargo.ServiceHttp.RateLimiting;
using Fargo.ServiceHttp.Routes;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddResponseCompression();

builder.Services.AddFargoRouteConstraints();

builder.Services.AddFargoOpenApi();

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.AddFargoJsonConverters());

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddFargoExceptionHandler();

builder.Services.AddProblemDetails();

builder.Services.AddFargoRateLimiter(builder.Configuration);

builder.Services.AddFargoForwardedHeaders(builder.Configuration);

builder.Services.AddHealthChecks().AddDbContextCheck<FargoDbContext>();

var app = builder.Build();

// Must run first so the real client IP and scheme (from the proxy) are seen by
// HTTPS redirection, authentication, and the rate limiter. No-op unless enabled
// in configuration (i.e. when actually deployed behind a proxy).
app.UseFargoForwardedHeaders();

app.UseExceptionHandler();

app.UseResponseCompression();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapFargoArticle();

app.MapFargoItem();

app.MapFargoItemEvent();

app.MapFargoUser();

app.MapFargoUserGroup();

app.MapFargoPartition();

app.MapFargoIdentity();

app.MapFargoAuditLog();

app.MapFargoDefaultEndpoints();

// Fail fast at startup if the JWT signing configuration is missing or weak, so
// the app never serves traffic with an insecure key. Skipped only during
// build-time OpenAPI document generation, which hosts the app through the
// "GetDocument.Insider" tool and runs without the JWT secret configured.
if (!string.Equals(
        System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
        "GetDocument.Insider",
        StringComparison.Ordinal))
{
    _ = app.Services
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<Fargo.Infrastructure.Security.JwtOptions>>()
        .Value;
}

app.Run();
