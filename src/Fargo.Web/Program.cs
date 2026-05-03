using Fargo.Api.Extensions;
using Fargo.ServiceDefaults;
using Fargo.Web;
using Fargo.Web.Components.Session;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();

// Aspire AppHost sets services__apiservice__http__0 to the actual resolved address
// (e.g. http://localhost:PORT in local dev, http://apiservice in containers).
var apiUrl = builder.Configuration["services:apiservice:http:0"] ?? "http://apiservice";

builder.Services.AddFargoSdk(o =>
{
    o.Server = apiUrl;
    o.ApiKey = builder.Configuration["Fargo:ApiKey"];
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
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Fargo.Web.Components.Layout.MainLayout).Assembly);

app.Run();
