using Fargo.Application.Extensions;
using Fargo.Http.Endpoints;
using Fargo.Http.ExceptionHandlers;
using Fargo.Http.JsonConverters;
using Fargo.Http.OpenApi;
using Fargo.Http.RateLimiting;
using Fargo.Http.Routes;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Fargo.ServiceDefaults;
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

builder.Services.AddFargoRateLimiter();

builder.Services.AddHealthChecks().AddDbContextCheck<FargoDbContext>();

var app = builder.Build();

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

app.MapFargoUser();

app.MapFargoUserGroup();

app.MapFargoPartition();

app.MapFargoIdentity();

app.MapFargoAuditLog();

app.MapFargoDefaultEndpoints();

app.Run();
