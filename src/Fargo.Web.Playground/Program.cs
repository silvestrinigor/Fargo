using Fargo.HttpClient;
using Fargo.Web.Playground;
using Fargo.Web.Playground.Components;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("Fargo.Web.Playground only runs in the Development environment.");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<PlaygroundAuthSession>();
builder.Services.AddFargoHttpClient(options =>
{
    var baseAddress = builder.Configuration["FargoHttpApi:BaseAddress"]
        ?? "http://localhost:5534";

    options.BaseAddress = new Uri(baseAddress);
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
