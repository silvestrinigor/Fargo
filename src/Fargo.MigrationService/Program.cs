using Fargo.Infrastructure.Persistence;
using Fargo.MigrationService;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(Worker.ActivitySourceName));

builder.AddSqlServerDbContext<FargoWriteDbContext>("Fargo");

var host = builder.Build();

host.Run();