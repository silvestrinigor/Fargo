using Fargo.Application.Requests.Commands;
using System.Diagnostics;

namespace Fargo.SeedService;

public class SeedService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime
        ) : BackgroundService
{
    public const string ActivitySourceName = "Seeds";

    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity(
                "Seeding database", ActivityKind.Client
                );

        try
        {
            using var scope = serviceProvider.CreateScope();

            var command = new InitializeSystemCommand(

                    );

            var handler = scope.ServiceProvider
                .GetRequiredService<InitializeSystemCommandHandler>();

            await handler.Handle(command, stoppingToken);

        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}