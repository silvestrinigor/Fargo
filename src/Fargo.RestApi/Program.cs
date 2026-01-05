using Fargo.HttpApi.Extensions;
using Fargo.Infrastructure.Converters;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new NameJsonConverter());
    options.SerializerOptions.Converters.Add(new DescriptionJsonConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapFargoArticle();

app.MapFargoItem();

app.MapFargoEvent();

await app.Services.InitInfrastructureAsync();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();