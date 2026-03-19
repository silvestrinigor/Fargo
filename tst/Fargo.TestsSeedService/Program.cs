using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.TestsSeedService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<TestsSeedWorker>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(TestsSeedWorker.ActivitySourceName));

var host = builder.Build();

host.Run();
