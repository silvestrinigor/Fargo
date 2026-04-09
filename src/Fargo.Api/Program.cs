using Fargo.Api.Extensions;
using Fargo.Api.Middlewares;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoHttpJsonOptions();

builder.Services.AddFargoTreeInfrastructure();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddFargoAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

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

app.MapFargoUserGroup();

app.MapFargoPartition();

app.MapFargoTree();

app.MapFargoAuthentication();

app.MapDefaultEndpoints();

app.Run();
