using Fargo.HttpApi.Articles;
using Fargo.HttpApi.Extensions;
using Fargo.HttpApi.Items;
using Fargo.HttpApi.Middlewares;
using Fargo.HttpApi.Partitions;
using Fargo.HttpApi.UserGroups;
using Fargo.HttpApi.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

builder.AddServiceDefaults();

builder.Services.ConfigureFargoRouting();

builder.Services.AddFargoOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureFargoHttpJsonOptions();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddFargoAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();

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

app.MapFargoAuthentication();

app.MapDefaultEndpoints();

app.Run();
