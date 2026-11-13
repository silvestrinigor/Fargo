using Fargo.Cli.Commands.Config;
using Fargo.Cli.Configurations;
using Fargo.Cli.ErrorHandling;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientRealtime;
using System.CommandLine;
using System.Net;

namespace Fargo.Cli.Commands.Realtime;

public sealed class WatchRealtimeCommand : Command
{
    public WatchRealtimeCommand(
        IFargoCliConfigurationStore configurationStore,
        FargoTokenRefresher tokenRefresher,
        ITokenStore tokenStore)
        : base("watch", """
            Print each change, as it happens, to the entities your user can read.

            Connects to the Fargo real-time service and keeps running until Ctrl+C.
            Only changes to entities in partitions you can access are shown; today
            these are article changes (created, edited, deleted). The line shows
            what changed; read the entity with the matching get command for its
            details.

            Changes are written to standard output; connection messages go to
            standard error, so --json output can be piped. Changes made while the
            connection is down are not replayed.

            The access token is refreshed automatically while watching. Requires
            a logged-in session ('fargo identity login') or --username/--password.

            Examples:
              fargo realtime watch
              fargo realtime watch --json
              fargo realtime watch --realtime-server https://localhost:7571
            """)
    {
        var realtimeServerOption = new Option<string?>("--realtime-server")
        {
            Description = "The real-time service URL. Defaults to 'fargo config get-realtime-server'."
        };

        var jsonOption = new Option<bool>("--json")
        {
            Description = "Write each change as a single-line JSON object."
        };

        Add(realtimeServerOption);
        Add(jsonOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var realtimeServerValue = parseResult.GetValue(realtimeServerOption);
            var json = parseResult.GetValue(jsonOption);

            Uri? realtimeServer;

            if (realtimeServerValue is not null)
            {
                if (!RealtimeServerUrl.TryParse(realtimeServerValue, out var parsed))
                {
                    Console.Error.WriteLine(
                        $"Invalid real-time server URL: '{realtimeServerValue}'. Provide an absolute http or https URL.");
                    return 1;
                }

                realtimeServer = parsed;
            }
            else
            {
                realtimeServer = configurationStore.Load().RealtimeServer;
            }

            if (realtimeServer is null)
            {
                Console.Error.WriteLine(
                    "No real-time server is configured. Set one with "
                    + "'fargo config set-realtime-server --url <url>' or pass --realtime-server <url>.");
                return 1;
            }

            if (await tokenStore.GetAsync(cancellationToken) is null)
            {
                Console.Error.WriteLine(
                    "Not logged in. Run 'fargo identity login' first, or pass --username and --password.");
                return 1;
            }

            // A watch can outlive the access token: the real-time service then
            // closes the connection, and the reconnect needs a fresh token. So
            // this command refreshes before expiry; other commands refresh once
            // at startup.
            var tokenSource = new RefreshingAccessTokenSource(tokenRefresher, tokenStore);

            await using var client = new FargoRealtimeClient(
                new FargoRealtimeClientOptions { RealtimeBaseAddress = realtimeServer },
                new DelegateRealtimeAccessTokenProvider(tokenSource.GetAccessTokenAsync));

            var output = new Lock();

            client.EntityChanged += notification =>
            {
                var line = json
                    ? RealtimeEventWriter.FormatJson(notification)
                    : RealtimeEventWriter.FormatText(notification);

                lock (output)
                {
                    Console.Out.WriteLine(line);
                    Console.Out.Flush();
                }

                return Task.CompletedTask;
            };

            client.Reconnecting += _ =>
            {
                Console.Error.WriteLine("Connection lost; reconnecting...");
                return Task.CompletedTask;
            };

            client.Resynchronized += () =>
            {
                Console.Error.WriteLine("Reconnected. Changes made while disconnected were not received.");
                return Task.CompletedTask;
            };

            try
            {
                await client.StartAsync(cancellationToken);
            }
            catch (HttpRequestException exception) when (
                exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                Console.Error.WriteLine(
                    "The real-time service rejected your session. Run 'fargo identity login' and try again.");
                ApiErrorReporter.WriteVerboseDiagnostics(exception);
                return 1;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested && IsConnectivityError(exception))
            {
                Console.Error.WriteLine(
                    $"Could not connect to the real-time service at {client.HubUri}. Make sure it is running "
                    + "and the URL is correct (check 'fargo config show' or pass --realtime-server <url>).");
                ApiErrorReporter.WriteVerboseDiagnostics(exception);
                return 1;
            }

            Console.Error.WriteLine($"Watching {client.HubUri}. Press Ctrl+C to stop.");

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ctrl+C: the normal way to end a watch.
            }

            await client.StopAsync(CancellationToken.None);

            Console.Error.WriteLine("Stopped.");

            return 0;
        });
    }

    private static bool IsConnectivityError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException or System.Net.Sockets.SocketException or IOException
                or TimeoutException or TaskCanceledException)
            {
                return true;
            }
        }

        return false;
    }
}
