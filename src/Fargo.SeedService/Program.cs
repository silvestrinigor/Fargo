using Fargo.Application.Identity;
using Fargo.Infrastructure.Extensions;
using Fargo.SeedService;
using Fargo.SeedService.Extensions;
using Fargo.SeedService.Security;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoDefaultAdmin(builder.Configuration);

builder.Services.AddFargoSeedInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddHostedService<SeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(SeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
