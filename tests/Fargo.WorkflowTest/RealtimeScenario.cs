using Fargo.ClientAuthentication;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.ClientRealtime;
using Fargo.RealtimeContracts;
using System.Collections.Concurrent;

namespace Fargo.WorkflowTest;

/// <summary>
/// End-to-end check of real-time push: API change → outbox → RabbitMQ →
/// real-time service → <see cref="FargoRealtimeClient"/>.
/// </summary>
/// <remarks>
/// Two users connect next to the admin: one with access to the article's
/// partition, one with access to an unrelated partition. The article's
/// created, edited and deleted events must reach the admin and the first user,
/// and never the second. Requires the outbox relay, the broker and the
/// real-time service to be running.
/// </remarks>
internal sealed class RealtimeScenario(
    FargoApiClient admin,
    ITokenStore adminTokens,
    WorkflowOptions options,
    string runTag,
    Check check)
{
    // Covers the outbox poll interval (1 s when idle) plus broker and push.
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(15);

    // How long to keep listening on the user that must receive nothing.
    private static readonly TimeSpan SilenceWindow = TimeSpan.FromSeconds(3);

    private const int ArticleTypeDefault = 1;

    public async Task RunAsync(Uri realtimeServer, CancellationToken cancellationToken)
    {
        Log.Step($"Real-time: connecting to {realtimeServer}");

        var zoneA = await CreatePartitionAsync("Realtime-A", cancellationToken);
        var zoneB = await CreatePartitionAsync("Realtime-B", cancellationToken);

        var (userAGuid, userATokens) = await CreateUserWithAccessAsync("rt-a", zoneA, cancellationToken);
        var (userBGuid, userBTokens) = await CreateUserWithAccessAsync("rt-b", zoneB, cancellationToken);

        var adminInbox = new Inbox();
        var userAInbox = new Inbox();
        var userBInbox = new Inbox();

        await using var adminClient = Connect(realtimeServer, new StoredAccessTokenSource(adminTokens), adminInbox);
        await using var userAClient = Connect(realtimeServer, new StoredAccessTokenSource(userATokens), userAInbox);
        await using var userBClient = Connect(realtimeServer, new StoredAccessTokenSource(userBTokens), userBInbox);

        await check.AllowedAsync("admin connects to the hub", () => adminClient.StartAsync(cancellationToken));
        await check.AllowedAsync("user with Realtime-A access connects", () => userAClient.StartAsync(cancellationToken));
        await check.AllowedAsync("user with Realtime-B access connects", () => userBClient.StartAsync(cancellationToken));

        Log.Step("Real-time: creating, editing and deleting an article in Realtime-A");

        var articleGuid = await admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = $"realtime-{runTag}",
                ArticleType = ArticleTypeDefault,
                PartitionsToAdd = [zoneA],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create the real-time article.");

        check.That(
            await adminInbox.WaitForAsync(articleGuid, EntityChangedNotification.ChangeTypes.Created, cancellationToken),
            "admin receives article created");
        check.That(
            await userAInbox.WaitForAsync(articleGuid, EntityChangedNotification.ChangeTypes.Created, cancellationToken),
            "user with partition access receives article created");

        await admin.V1.Articles[articleGuid.ToString()].PatchAsync(
            new ArticleUpdateDto { Name = $"realtime-renamed-{runTag}" },
            cancellationToken: cancellationToken);

        check.That(
            await userAInbox.WaitForAsync(articleGuid, EntityChangedNotification.ChangeTypes.Edited, cancellationToken),
            "user with partition access receives article edited");

        await admin.V1.Articles[articleGuid.ToString()].DeleteAsync(cancellationToken: cancellationToken);

        check.That(
            await userAInbox.WaitForAsync(articleGuid, EntityChangedNotification.ChangeTypes.Deleted, cancellationToken),
            "user with partition access receives article deleted");

        // Every event has reached user A by now; give user B the same chance.
        await Task.Delay(SilenceWindow, cancellationToken);

        check.That(
            !userBInbox.Received.Any(n => n.Guid == articleGuid),
            "user without access to the article's partitions receives nothing about it");

        Log.Step("Real-time: cleaning up");

        await adminClient.StopAsync(cancellationToken);
        await userAClient.StopAsync(cancellationToken);
        await userBClient.StopAsync(cancellationToken);

        await check.AllowedAsync("admin deletes rt-a user", () =>
            admin.V1.Users[userAGuid].DeleteAsync(cancellationToken: cancellationToken));
        await check.AllowedAsync("admin deletes rt-b user", () =>
            admin.V1.Users[userBGuid].DeleteAsync(cancellationToken: cancellationToken));
        await check.AllowedAsync("admin deletes Realtime-A partition", () =>
            admin.V1.Partitions[zoneA].DeleteAsync(cancellationToken: cancellationToken));
        await check.AllowedAsync("admin deletes Realtime-B partition", () =>
            admin.V1.Partitions[zoneB].DeleteAsync(cancellationToken: cancellationToken));
    }

    private static FargoRealtimeClient Connect(Uri realtimeServer, IFargoAccessTokenSource tokens, Inbox inbox)
    {
        var client = new FargoRealtimeClient(new FargoRealtimeClientOptions { RealtimeBaseAddress = realtimeServer }, tokens);

        client.EntityChanged += notification =>
        {
            inbox.Received.Enqueue(notification);

            return Task.CompletedTask;
        };

        return client;
    }

    private async Task<Guid> CreatePartitionAsync(string name, CancellationToken cancellationToken)
    {
        var guid = await admin.V1.Partitions.PostAsync(
            new PartitionCreateDto { Name = $"{name}-{runTag}" },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create partition {name}.");

        Log.Success($"Partition {name} {guid.ToString("N")[..8]}");

        return guid;
    }

    /// <summary>
    /// Creates a user with access to one partition, clears its first-login
    /// password change, logs it in, and returns its own token store.
    /// </summary>
    private async Task<(Guid Guid, ITokenStore Tokens)> CreateUserWithAccessAsync(
        string baseName,
        Guid partitionGuid,
        CancellationToken cancellationToken)
    {
        var nameid = $"{baseName}-{runTag}";

        var guid = await admin.V1.Users.PostAsync(
            new UserCreateDto
            {
                Nameid = nameid,
                Description = "Real-time scenario user.",
                IsActive = true,
                Authentication = new UserAuthenticationCreateDto
                {
                    Password = options.NewUserPassword,
                    DefaultPasswordExpirationPeriod = "365.00:00:00",
                },
                // Reading articles needs only partition access.
                PartitionAccessesToAdd = [partitionGuid],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create user {nameid}.");

        await admin.V1.Identity.Password.PutAsync(
            new IdentityPasswordUpdateDto
            {
                Nameid = nameid,
                CurrentPassword = options.NewUserPassword,
                NewPassword = options.NewUserPassword,
            },
            cancellationToken: cancellationToken);

        var auth = await admin.V1.Identity.Login.PostAsync(
            new IdentityLoginDto { Nameid = nameid, Password = options.NewUserPassword },
            cancellationToken: cancellationToken);

        if (auth?.AccessToken is null || auth.RefreshToken is null || auth.ExpiresAt is null)
        {
            throw new InvalidOperationException($"Login for '{nameid}' did not return a complete token set.");
        }

        var tokens = new TokenStore();
        await tokens.SetAsync(new AuthTokens(auth.AccessToken, auth.RefreshToken, auth.ExpiresAt.Value), cancellationToken);

        Log.Success($"User {nameid} {guid.ToString("N")[..8]}");

        return (guid, tokens);
    }

    private sealed class Inbox
    {
        public ConcurrentQueue<EntityChangedNotification> Received { get; } = new();

        public async Task<bool> WaitForAsync(Guid entityGuid, string changeType, CancellationToken cancellationToken)
        {
            var deadline = DateTime.UtcNow + DeliveryTimeout;

            while (DateTime.UtcNow < deadline)
            {
                if (Received.Any(n => n.Guid == entityGuid && n.ChangeType == changeType))
                {
                    return true;
                }

                await Task.Delay(100, cancellationToken);
            }

            return false;
        }
    }
}
