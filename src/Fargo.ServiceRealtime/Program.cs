using Fargo.Application.DependencyInjection;
using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.Infrastructure.Persistence;
using Fargo.ServiceDefaults;
using Fargo.ServiceRealtime.Authentication;
using Fargo.ServiceRealtime.Connections;
using Fargo.ServiceRealtime.Hubs;
using Fargo.ServiceRealtime.Notifications;
using Fargo.ServiceRealtime.Options;
using Fargo.ServiceRealtime.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddFargoServiceDefaults();

// The application layer provides ActorResolver; the infrastructure layer the
// repositories it reads and the Fargo JWT validation.
builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

// The broker health check is disabled on purpose: while the broker is down the
// hub keeps serving connections and the consumer keeps retrying with backoff.
builder.AddRabbitMQClient("fargo-rabbitmq", settings => settings.DisableHealthChecks = true);

builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer();

builder.Services.AddHubAccessTokenFromQueryString();

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services
.AddOptions<RealtimeOptions>()
.Bind(builder.Configuration.GetSection(RealtimeOptions.SectionName))
.ValidateDataAnnotations()
.ValidateOnStart();

builder.Services.AddSingleton<RealtimeConnectionRegistry>();

builder.Services.AddSingleton<IArticleEventHandler, ArticleEventHubNotifier>();

builder.Services.AddFargoArticleEventConsumer(builder.Configuration);

builder.Services.AddHostedService<RealtimeAccessRefreshService>();

builder.Services.AddHealthChecks().AddDbContextCheck<FargoDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<FargoHub>(FargoHub.Path, options =>
{
    // Close the connection when its access token expires; the client
    // reconnects with a fresh token, which is validated again.
    options.CloseOnAuthenticationExpiration = true;
});

app.MapFargoDefaultEndpoints();

// Fail fast at startup if the JWT signing configuration is missing or weak.
_ = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<Fargo.Infrastructure.Security.JwtOptions>>()
    .Value;

app.Run();
