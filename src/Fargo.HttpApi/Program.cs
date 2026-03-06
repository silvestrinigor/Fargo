using Fargo.HttpApi.Extensions;
using Fargo.HttpApi.Middlewares;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var defaultAdminNameid = builder.Configuration.GetApplicationConfiguration("DefaultAdminNameid");

var defaultAdminPassword = builder.Configuration.GetApplicationConfiguration("DefaultAdminPassword");

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoHttpJsonOptions();

builder.Services.AddFargoScopes();

builder.Services.AddFargoWriteDbContext(connectionString);

builder.Services.AddFargoReadDbContext(connectionString);

builder.Services.AddFargoAuthentication(builder.Configuration);

var app = builder.Build();

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

await app.Services.InitializeSystem(defaultAdminNameid, defaultAdminPassword);

app.Run();