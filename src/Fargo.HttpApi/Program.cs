using Fargo.Application.Extensions;
using Fargo.HttpApi.ExceptionHandlers;
using Fargo.HttpApi.JsonConfig;
using Fargo.HttpApi.OpenApiExtensions;
using Fargo.HttpApi.Endpoints;
using Fargo.HttpApi.RouteConstraints;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddResponseCompression();

builder.Services.AddFargoRouteConstraints();

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

app.MapFargoDefaultEndpoints();

app.Run();
