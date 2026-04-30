using Fargo.Api;
using Fargo.Api.Extensions;
using Fargo.Api.Hubs;
using Fargo.Api.Middlewares;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddScoped<IFargoEventPublisher, SignalREventPublisher>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

builder.AddServiceDefaults();

builder.Services.AddFargoOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoHttpJsonOptions();

builder.Services.AddFargoTreeInfrastructure();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddFargoAuthentication(builder.Configuration);

builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection(ApiClientOptions.SectionName));

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();

app.UseMiddleware<FargoExceptionMiddleware>();

app.UseMiddleware<ApiClientMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapFargoApiClient();

app.MapFargoArticle();

app.MapFargoItem();

app.MapFargoUser();

app.MapFargoUserGroup();

app.MapFargoPartition();

app.MapFargoEvent();

app.MapFargoTree();

app.MapFargoAuthentication();

app.MapDefaultEndpoints();

app.MapHub<FargoEventHub>("/hub/events");

app.Run();
