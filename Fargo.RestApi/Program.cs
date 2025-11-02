using Fargo.HttpApi.Maps;
using Fargo.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapFagoArticle();

app.Services.InitInfrastructure();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();