using Fargo.HttpApi.Client.Extensions;
using Fargo.WebPlayground.Components;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
.AddRazorComponents()
.AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

var baseAddress = builder.Configuration["FargoHttpApi:BaseAddress"] ?? "http://localhost:5534";

builder.Services.AddFargoHttpApiClient(new Uri(baseAddress));

var app = builder.Build();

if (app.Environment.EnvironmentName == Environments.Development)
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

// A call to UseAntiforgery must be placed after calls, if present, to UseAuthentication and UseAuthorization.
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
