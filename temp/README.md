# Fargo MCP Tester

Throwaway test bench, not part of the solution. A Blazor Server app (Microsoft
Fluent UI) where you log in with your Fargo credentials and chat with an LLM that
calls the Fargo MCP tools **as you**.

```
Browser ──► Fargo.McpTester ──► LLM (GitHub Models)
               │  login            
               ▼                   
           Fargo API ◄── Fargo.Mcp (--transport http) ◄── MCP over HTTP + your bearer token
```

`Fargo.McpTester` is the MCP host: it holds one MCP client per logged-in browser
tab (circuit) and sends your current access token on every MCP request.

## Run

1. Start the Fargo API (for example through the Aspire AppHost).
2. Start the MCP server in HTTP mode:

   ```powershell
   $env:FARGO_SERVER = "http://localhost:5534"   # your Fargo API
   dotnet run --project src/client/Fargo.Mcp -- --transport http
   ```

3. Give the tester an LLM token (GitHub personal access token with the
   `models:read` permission), once:

   ```powershell
   dotnet user-secrets set "Llm:Token" "<token>" --project temp/Fargo.McpTester
   ```

4. Start the tester and open the printed URL:

   ```powershell
   dotnet run --project temp/Fargo.McpTester
   ```

Log in, then use the **Chat** tab. The **Tools** tab lists what the MCP server
exposes to your user.

## Configuration (`temp/Fargo.McpTester/appsettings.json`)

| Key | Default | Meaning |
| --- | --- | --- |
| `Fargo:ApiUrl` | `http://localhost:5534` | Fargo API used for login/refresh |
| `Mcp:Url` | `http://localhost:5290/mcp` | The Fargo.Mcp HTTP endpoint |
| `Llm:Endpoint` | `https://models.github.ai/inference` | Any OpenAI-compatible endpoint |
| `Llm:Model` | `openai/gpt-4o-mini` | Must support tool calling |
| `Llm:Token` | (user-secrets) | API key / token for the endpoint |

To use Ollama instead: `Llm:Endpoint` = `http://localhost:11434/v1`, `Llm:Model` =
a tool-capable model such as `llama3.1`; no token is needed for a loopback endpoint.

## Notes

- Tools that create, update, delete or move data are hidden from the model until
  you switch on **Allow tools that change data**.
- The login lives in the browser tab's circuit: a page refresh logs you out.
- Passwords are never stored or logged; tokens are kept in server memory only.
