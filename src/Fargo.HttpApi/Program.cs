using Fargo.Application.DependencyInjection;
using Fargo.HttpApi.Endpoints;
using Fargo.HttpApi.ExceptionHandlers;
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

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddFargoExceptionHandler();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseResponseCompression();

app.UseExceptionHandler();

app.UseStatusCodePages();

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
