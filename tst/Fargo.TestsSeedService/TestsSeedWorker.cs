using Fargo.Application.Commands;
using Fargo.Application.Commands.ArticleCommands;
using Fargo.Application.Models.ArticleModels;
using Fargo.Domain.Services;
using System.Diagnostics;

namespace Fargo.TestsSeedService;

public sealed class TestsSeedWorker(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime
        ) : BackgroundService
{
    public const string ActivitySourceName = "TestSeeds";

    private static readonly ActivitySource activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity(
                "Seeding database", ActivityKind.Client
                );

        try
        {
            using var scope = serviceProvider.CreateScope();

            var createArticleHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<ArticleCreateCommand, Guid>>();

            ArticleCreateCommand createArticleCommand;

            ArticleCreateModel createArticleModel;

            createArticleModel = new ArticleCreateModel(
                new("Apple iPad 11-inch"),
                new("""
                WHY IPAD — The 11-inch iPad is now more capable than ever with the 
                superfast A16 chip, a stunning Liquid Retina display, advanced cameras, 
                fast Wi-Fi, USB-C connector, and four gorgeous colors.* iPad delivers 
                a powerful way to create, stay connected, and get things done.
                """),
                PartitionService.GlobalPartitionGuid
            );

            createArticleCommand = new ArticleCreateCommand(createArticleModel);

            await createArticleHandler.Handle(createArticleCommand, stoppingToken);

        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}
