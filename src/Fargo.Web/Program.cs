using Fargo.Infrastructure.Client.Extensions;
using Fargo.ServiceDefaults;
using Fargo.Web.Api;
using Fargo.Web.Components;
using Fargo.Web.Features.Auth;
using Fargo.Web.Features.Partitions;
using Fargo.Web.Features.Users;
using Fargo.Web.Security;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IClientSessionStore, BrowserClientSessionStore>();
builder.Services.AddScoped<ClientSessionAccessor>();

builder.Services.AddFargoHttpApiClient(
    configureClient: client =>
        client.AddHttpMessageHandler<FargoApiAuthorizationHandler>());

builder.Services.AddScoped<FargoApiClientFactory>();

builder.Services.AddScoped<AuthenticationApi>();
builder.Services.AddScoped<PartitionApi>();
builder.Services.AddScoped<UserApi>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
