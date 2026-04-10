using Fargo.Sdk;
using Fargo.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Fargo.Web.Api;
using Fargo.Web.Components;
using Fargo.Web.Extensions;
using Fargo.Web.Features.Partitions;
using Fargo.Web.Features.Trees;
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

builder.Services.ConfigureFargoWebHttpJsonOptions();

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

builder.Services.AddHttpClient("FargoApi", client =>
{
    client.BaseAddress = new Uri("http://apiservice");
});

builder.Services.AddScoped<PartitionApi>();
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<PartitionTreeApi>();
builder.Services.AddScoped<UserGroupTreeApi>();
builder.Services.AddScoped<ArticleTreeApi>();
builder.Services.AddScoped<PartitionSecurityTreeApi>();

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
