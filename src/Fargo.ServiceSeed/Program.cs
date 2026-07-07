using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceSeed;
using Fargo.ServiceSeed.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoDefaultAdminOptions(builder.Configuration);

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(
    builder.Configuration, configure => configure.UseSystemCurrentActor());

builder.Services.AddHostedService<SeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(SeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
