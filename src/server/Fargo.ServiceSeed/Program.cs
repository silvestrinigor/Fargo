using Fargo.Application.Operations.Initialization;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceSeed.Extensions;
using Fargo.ServiceSeed.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoSeedOptions(builder.Configuration);

builder.Services.AddFargoDbContext(builder.Configuration);

builder.Services.AddFargoUnitOfWork();

builder.Services.AddFargoRepositories();

builder.Services.AddFargoSecurity();

builder.Services.AddFargoInitializationApplication();

builder.Services.AddHostedService<FargoSeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoSeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
