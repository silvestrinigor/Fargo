using Fargo.Application.Commom;
using Fargo.HttpApi.Extensions;
using Fargo.HttpApi.Middlewares;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Fargo");

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddFargoOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoHttpJsonOptions();

builder.Services.AddFargoScopes();

builder.Services.AddFargoCurrentUser();

builder.Services.AddFargoWriteRepositiresScopes();

builder.Services.AddFargoReadRepositoriesScopes();

builder.Services.AddFargoDomainServiceScopes();

builder.Services.AddFargoPasswordHasher();

builder.Services.AddFargoUnitOfWork();

builder.Services.AddFargoWriteDbContext(connectionString);

builder.Services.AddFargoReadDbContext(connectionString);

builder.Services.AddFargoAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

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

app.MapFargoAuthentication();

app.MapDefaultEndpoints();

app.Run();