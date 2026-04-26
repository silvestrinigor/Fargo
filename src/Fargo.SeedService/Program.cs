using Fargo.Infrastructure.Extensions;
using Fargo.SeedService;
using Fargo.SeedService.Extensions;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoDefaultAdmin(builder.Configuration);

builder.Services.AddFargoApiClientSeed(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddFargoSeedInfrastructure(builder.Configuration);

builder.Services.AddHostedService<SeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(SeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
