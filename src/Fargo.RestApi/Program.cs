using Fargo.HttpApi.EndpointRouteBuilder;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.ServiceDefaults;

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

app.MapFargoArea();

app.MapFargoArticle();

app.MapFargoContainer();

app.MapFargoPartition();

app.Services.InitInfrastructure();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();