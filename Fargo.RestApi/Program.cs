using Fargo.HttpApi.Maps;
using Fargo.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapFagoArticle();

app.Services.InitInfrastructure();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();