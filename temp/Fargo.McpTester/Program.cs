using Fargo.ClientHttp.Extensions;
using Fargo.McpTester.Components;
using Fargo.McpTester.Services;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

var apiUrl = builder.Configuration["Fargo:ApiUrl"]
    ?? throw new InvalidOperationException("Configuration 'Fargo:ApiUrl' is required.");

// Only used for the anonymous login/refresh calls; the user's token is held in
// UserSession (one per Blazor circuit), never in the shared singleton token store.
builder.Services.AddFargoHttpClient(new Uri(apiUrl));

// Shared and stateless: the LLM client holds no user data.
builder.Services.AddSingleton<LlmChatClientProvider>();

// One instance per circuit (= per browser tab): token, MCP connection and chat history.
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<McpSession>();
builder.Services.AddScoped<ChatService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
