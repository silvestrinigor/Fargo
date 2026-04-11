using Fargo.Sdk;
using Fargo.ServiceDefaults;
using Fargo.Web.Api;
using Fargo.Web.Components;
using Fargo.Web.Security;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<BrowserSdkSessionStore>();
builder.Services.AddScoped<IEngine>(sp =>
{
    // Aspire AppHost sets services__apiservice__http__0 to the actual resolved address
    // (e.g. http://localhost:PORT in local dev, http://apiservice in containers).
    // Reading it directly bypasses IHttpClientFactory service discovery and avoids
    // storing a factory-managed HttpClient long-term (lifetime/recycling issues).
    var apiUrl = sp.GetRequiredService<IConfiguration>()["services:apiservice:http:0"]
                 ?? "http://apiservice";

    var engine = new Engine(
        sp.GetService<ILoggerFactory>(),
        sp.GetRequiredService<BrowserSdkSessionStore>()
    );

    engine.Configure(apiUrl);

    return engine;
});
builder.Services.AddScoped<FargoSession>();

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
