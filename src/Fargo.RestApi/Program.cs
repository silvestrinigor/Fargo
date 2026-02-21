using Fargo.HttpApi.Extensions;
using Fargo.Infrastructure.Converters;
using Fargo.Infrastructure.Converters.ValueObjectsJsonConverters;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Fargo.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new NameJsonConverter());
    options.SerializerOptions.Converters.Add(new DescriptionJsonConverter());
    options.SerializerOptions.Converters.Add(new LimitJsonConverter());
    options.SerializerOptions.Converters.Add(new PageJsonConverter());
});

builder.Services.AddAuthorization();

builder.AddServiceDefaults();


builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("FargoDatabase");

builder.Services.AddDbContext<FargoWriteDbContext>(opt =>
    opt.UseSqlServer(
        connectionString
    ));

builder.Services.AddDbContext<FargoReadDbContext>(opt =>
    opt.UseSqlServer(
        connectionString
    ));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapFargoArticle();

app.MapFargoItem();

app.MapFargoUser();

app.MapFargoPartition();

app.MapFargoAuthentication();

await app.Services.InitInfrastructureAsync();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();