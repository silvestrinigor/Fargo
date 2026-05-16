using Fargo.Application.Articles;
using Fargo.Core.Barcodes;
using NSubstitute;
using UnitsNet;

namespace Fargo.Application.Tests.Articles;

public sealed class ArticleApplicationServiceTests
{
    [Fact]
    public async Task Patch_Should_NotInvokeGeneralCommands_WhenNameAndDescriptionAreNull()
    {
        var fixture = new Fixture();
        var articleGuid = Guid.NewGuid();

        await fixture.Sut.Patch(
            articleGuid,
            new ArticlePatchDto(Name: null, Description: null));

        await fixture.RenameHandler.DidNotReceiveWithAnyArgs().Handle(default!, default);
        await fixture.ChangeDescriptionHandler.DidNotReceiveWithAnyArgs().Handle(default!, default);
        await fixture.UnitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Patch_Should_InvokeSetMetrics_WhenMetricsDtoIsNotNull()
    {
        var fixture = new Fixture();
        var articleGuid = Guid.NewGuid();
        var metrics = new ArticleMetricsDto(Mass.FromKilograms(2), Length.FromCentimeters(10));

        await fixture.Sut.Patch(articleGuid, new ArticlePatchDto(Metrics: metrics));

        await fixture.SetMetricsHandler.Received(1).Handle(
            Arg.Is<ArticleSetMetricsCommand>(command =>
                command.ArticleGuid == articleGuid &&
                command.Metrics == metrics.ToCore()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Patch_Should_InvokeSetBarcodes_WhenBarcodesDtoIsNotNull()
    {
        var fixture = new Fixture();
        var articleGuid = Guid.NewGuid();
        var barcodes = new ArticleBarcodesDto(Ean13: new Ean13("1234567890123"));

        await fixture.Sut.Patch(articleGuid, new ArticlePatchDto(Barcodes: barcodes));

        await fixture.SetBarcodesHandler.Received(1).Handle(
            Arg.Is<ArticleSetBarcodesCommand>(command =>
                command.ArticleGuid == articleGuid &&
                command.Barcodes == barcodes.ToCore()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Patch_Should_InvokeSetPartitions_WhenPartitionsCollectionIsEmpty()
    {
        var fixture = new Fixture();
        var articleGuid = Guid.NewGuid();
        IReadOnlyCollection<Guid> partitions = [];

        await fixture.Sut.Patch(articleGuid, new ArticlePatchDto(Partitions: partitions));

        await fixture.SetPartitionsHandler.Received(1).Handle(
            Arg.Is<ArticleSetPartitionsCommand>(command =>
                command.ArticleGuid == articleGuid &&
                command.PartitionGuids.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Patch_Should_NotInvokeSetPartitions_WhenPartitionsCollectionIsNull()
    {
        var fixture = new Fixture();

        await fixture.Sut.Patch(Guid.NewGuid(), new ArticlePatchDto(Partitions: null));

        await fixture.SetPartitionsHandler.DidNotReceiveWithAnyArgs().Handle(default!, default);
    }

    private sealed class Fixture
    {
        public ICommandHandler<ArticleCreateCommand, Guid> CreateArticleHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreateCommand, Guid>>();

        public ICommandHandler<ArticleChangeDescriptionCommand> ChangeDescriptionHandler { get; } =
            Substitute.For<ICommandHandler<ArticleChangeDescriptionCommand>>();

        public ICommandHandler<ArticleSetShelfLifeCommand> SetShelfLifeHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetShelfLifeCommand>>();

        public ICommandHandler<ArticleSetColorCommand> SetColorHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetColorCommand>>();

        public ICommandHandler<ArticleSetMetricsCommand> SetMetricsHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetMetricsCommand>>();

        public ICommandHandler<ArticleSetBarcodesCommand> SetBarcodesHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetBarcodesCommand>>();

        public ICommandHandler<ArticleSetPartitionsCommand> SetPartitionsHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetPartitionsCommand>>();

        public ICommandHandler<ArticleActivateCommand> ActivateHandler { get; } =
            Substitute.For<ICommandHandler<ArticleActivateCommand>>();

        public ICommandHandler<ArticleDeactivateCommand> DeactivateHandler { get; } =
            Substitute.For<ICommandHandler<ArticleDeactivateCommand>>();

        public ICommandHandler<ArticleRenameCommand> RenameHandler { get; } =
            Substitute.For<ICommandHandler<ArticleRenameCommand>>();

        public ICommandHandler<ArticleDeleteCommand> DeleteHandler { get; } =
            Substitute.For<ICommandHandler<ArticleDeleteCommand>>();

        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

        public ArticleApplicationService Sut { get; }

        public Fixture()
        {
            CreateArticleHandler.Handle(Arg.Any<ArticleCreateCommand>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());

            Sut = new ArticleApplicationService(
                CreateArticleHandler,
                ChangeDescriptionHandler,
                SetShelfLifeHandler,
                SetColorHandler,
                SetMetricsHandler,
                SetBarcodesHandler,
                SetPartitionsHandler,
                ActivateHandler,
                DeactivateHandler,
                RenameHandler,
                DeleteHandler,
                UnitOfWork);
        }
    }
}
