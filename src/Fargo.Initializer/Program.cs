using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.Initializer.Options;
using Fargo.Initializer.Services;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoInitializerOptions(builder.Configuration);

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddHostedService<FargoInitializerService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoInitializerService.ActivitySourceName));

var host = builder.Build();

host.Run();
