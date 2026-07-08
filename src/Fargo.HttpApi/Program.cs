using Fargo.Application.DependencyInjection;
using Fargo.HttpApi.Endpoints;
using Fargo.HttpApi.Json;
using Fargo.HttpApi.Middlewares;
using Fargo.HttpApi.OpenApi;
using Fargo.HttpApi.Routes;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddResponseCompression();

builder.Services.AddFargoRoutes();

builder.Services.AddFargoOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoJson();

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration, configure => configure.UseHttpCurrentActor());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();

app.UseMiddleware<FargoExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapFargoArticle();

app.MapFargoItem();

app.MapFargoUser();

app.MapFargoUserGroup();

app.MapFargoPartition();

app.MapFargoIdentity();

app.MapDefaultEndpoints();

app.Run();
