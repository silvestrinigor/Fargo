using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Fargo.McpTester.Services;

/// <summary>
/// Builds the shared LLM client from configuration. Defaults target GitHub Models
/// (OpenAI-compatible); any OpenAI-compatible endpoint (OpenAI, Azure OpenAI v1,
/// Ollama at http://localhost:11434/v1) works by changing Llm:Endpoint / Llm:Model.
/// The API key is read from Llm:Token (set it with user-secrets, never in a file).
/// </summary>
public sealed class LlmChatClientProvider
{
    public LlmChatClientProvider(IConfiguration configuration)
    {
        var endpoint = configuration["Llm:Endpoint"];
        var model = configuration["Llm:Model"];
        var token = configuration["Llm:Token"];

        // Ollama and other local servers ignore the key, so only require it for remote endpoints.
        var isLocal = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.IsLoopback;

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(model))
        {
            Problem = "Configuration 'Llm:Endpoint' and 'Llm:Model' are required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(token) && !isLocal)
        {
            Problem = "No LLM token configured. Run: dotnet user-secrets set \"Llm:Token\" \"<your GitHub token>\" " +
                      "in temp/Fargo.McpTester.";
            return;
        }

        var openAi = new OpenAIClient(
            new ApiKeyCredential(string.IsNullOrWhiteSpace(token) ? "not-needed" : token),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

        Client = new ChatClientBuilder(openAi.GetChatClient(model).AsIChatClient())
            .UseFunctionInvocation()
            .Build();

        ModelName = model;
    }

    /// <summary>The chat client with automatic tool invocation, or null when not configured.</summary>
    public IChatClient? Client { get; }

    public string? ModelName { get; }

    /// <summary>Why <see cref="Client"/> is unavailable, shown in the UI.</summary>
    public string? Problem { get; }
}
