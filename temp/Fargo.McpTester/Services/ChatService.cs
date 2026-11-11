using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Fargo.McpTester.Services;

public enum ChatEntryKind { User, Assistant, ToolCall, Error }

public sealed class ChatEntry(ChatEntryKind kind, string text)
{
    public ChatEntryKind Kind { get; } = kind;
    public string Text { get; set; } = text;

    // Tool call details (Kind == ToolCall).
    public string? CallId { get; init; }
    public string? ToolName { get; init; }
    public string? Arguments { get; init; }
    public string? Result { get; set; }
}

/// <summary>
/// The conversation of one Blazor circuit: sends messages to the LLM together
/// with the MCP tools and records the tool calls the model makes.
/// </summary>
public sealed class ChatService
{
    private const int MaxResultLength = 4000;

    private const string SystemPrompt =
        "You are an assistant for the Fargo inventory system. Use the available tools to answer " +
        "questions about articles, items, partitions, users and audit logs. Prefer reading data over " +
        "guessing. Before any destructive action, state what you are about to do.";

    // Tools that change data. Hidden from the model unless the user allows writes.
    private static readonly string[] WriteMarkers = ["_create", "_update", "_delete", "_rotate", "_move"];

    private readonly LlmChatClientProvider _llm;
    private readonly McpSession _mcp;
    private readonly List<ChatMessage> _history = [new(ChatRole.System, SystemPrompt)];

    public ChatService(LlmChatClientProvider llm, McpSession mcp)
    {
        _llm = llm;
        _mcp = mcp;
    }

    public event Action? Changed;

    public List<ChatEntry> Entries { get; } = [];

    public bool IsBusy { get; private set; }

    /// <summary>When false (default), tools that create, update or delete data are not offered to the model.</summary>
    public bool AllowWrites { get; set; }

    public string? Problem => _llm.Problem;

    public string? ModelName => _llm.ModelName;

    public void Clear()
    {
        Entries.Clear();
        _history.RemoveRange(1, _history.Count - 1);
        Changed?.Invoke();
    }

    public async Task SendAsync(string text, CancellationToken cancellationToken = default)
    {
        if (IsBusy || string.IsNullOrWhiteSpace(text) || _llm.Client is null)
        {
            return;
        }

        IsBusy = true;
        Entries.Add(new ChatEntry(ChatEntryKind.User, text));
        _history.Add(new ChatMessage(ChatRole.User, text));
        Changed?.Invoke();

        var updates = new List<ChatResponseUpdate>();

        try
        {
            var tools = (await _mcp.GetToolsAsync(cancellationToken))
                .Where(t => AllowWrites || !IsWriteTool(t.Name))
                .Cast<AITool>()
                .ToList();

            ChatEntry? current = null;

            await foreach (var update in _llm.Client.GetStreamingResponseAsync(
                _history, new ChatOptions { Tools = tools }, cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case TextContent { Text.Length: > 0 } textContent:
                            if (current is null)
                            {
                                current = new ChatEntry(ChatEntryKind.Assistant, string.Empty);
                                Entries.Add(current);
                            }

                            current.Text += textContent.Text;
                            break;

                        case FunctionCallContent call:
                            current = null;
                            Entries.Add(new ChatEntry(ChatEntryKind.ToolCall, call.Name)
                            {
                                CallId = call.CallId,
                                ToolName = call.Name,
                                Arguments = Format(call.Arguments)
                            });
                            break;

                        case FunctionResultContent result:
                            var entry = Entries.LastOrDefault(e => e.CallId == result.CallId);
                            if (entry is not null)
                            {
                                entry.Result = Truncate(Format(result.Result));
                            }

                            break;
                    }
                }

                Changed?.Invoke();
            }

            _history.AddMessages(updates);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Entries.Add(new ChatEntry(ChatEntryKind.Error, ex.Message));
        }
        finally
        {
            IsBusy = false;
            Changed?.Invoke();
        }
    }

    internal static bool IsWriteTool(string name) =>
        WriteMarkers.Any(m => name.Contains(m, StringComparison.OrdinalIgnoreCase));

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        string s => s,
        _ => JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })
    };

    private static string Truncate(string value) =>
        value.Length <= MaxResultLength ? value : value[..MaxResultLength] + "\n… (truncated)";
}
