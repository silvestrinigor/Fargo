using Fargo.HttpApi.Maps;
using Fargo.ServiceDefaults;
using Fargo.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapFargoArticle();

app.MapFargoContainer();

app.Services.InitInfrastructure();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();