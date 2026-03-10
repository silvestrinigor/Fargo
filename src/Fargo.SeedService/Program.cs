using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Extensions;
using Fargo.SeedService;
using Fargo.ServiceDefaults;
using Fargo.SeedService.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFargoDefaultAdmin(builder.Configuration);

builder.Services.AddFargoWriteRepositoriesScopes();

builder.Services.AddFargoInitializeSystemScope();

builder.Services.AddFargoDomainServiceScopes();

builder.Services.AddFargoPasswordHasher();

builder.Services.AddFargoUnitOfWork();

builder.Services.AddHostedService<SeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(SeedService.ActivitySourceName));

builder.AddSqlServerDbContext<FargoWriteDbContext>("Fargo");

var host = builder.Build();

host.Run();