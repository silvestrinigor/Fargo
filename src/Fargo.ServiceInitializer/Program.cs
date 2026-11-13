using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.ServiceDefaults;
using Fargo.ServiceInitializer.Options;
using Fargo.ServiceInitializer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoInitializerOptions(builder.Configuration);

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddHostedService<FargoInitializerService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoInitializerService.ActivitySourceName));

var host = builder.Build();

host.Run();
