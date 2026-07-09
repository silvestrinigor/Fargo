using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceSeed;
using Fargo.ServiceSeed.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoSeedOptions(builder.Configuration);

builder.Services.AddFargoSystemApplication();

builder.Services.AddFargoConnectionStringOptions(builder.Configuration);

builder.Services.AddFargoDbContext();

builder.Services.AddFargoUnitOfWork();

builder.Services.AddFargoRepositories();

builder.Services.AddHostedService<SeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(SeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
