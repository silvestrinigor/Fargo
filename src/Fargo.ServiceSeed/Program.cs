using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceSeed;
using Fargo.ServiceSeed.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoSeedOptions(builder.Configuration);

builder.Services.AddFargoConnectionStringOptions(builder.Configuration);

builder.Services.AddFargoDbContext();

builder.Services.AddFargoUnitOfWork();

builder.Services.AddFargoRepositories();

builder.Services.AddFargoSecurity();

builder.Services.AddFargoSystemApplication();

builder.Services.AddHostedService<FargoSeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoSeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
