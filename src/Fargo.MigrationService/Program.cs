using Fargo.Application.Identity;
using Fargo.Infrastructure.Extensions;
using Fargo.MigrationService;
using Fargo.MigrationService.Security;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(MigrationService.ActivitySourceName));

builder.Services.AddFargoMigrationInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var host = builder.Build();

host.Run();
