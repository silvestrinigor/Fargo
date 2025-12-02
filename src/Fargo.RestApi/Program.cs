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

app.MapFargoArticle();

app.MapFargoContainer();

app.MapFargoItem();

await app.Services.InitInfrastructureAsync();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();