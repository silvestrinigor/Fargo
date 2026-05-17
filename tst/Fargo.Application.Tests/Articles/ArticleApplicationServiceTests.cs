using Fargo.Application.Articles;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using NSubstitute;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

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

    [Fact]
    public async Task Create_Should_InvokeVariationCreateHandler_WhenKindIsVariation()
    {
        var fixture = new Fixture();
        var fromArticleGuid = Guid.NewGuid();

        await fixture.Sut.Create(
            new ArticleCreateDto(
                new Name("Variation"),
                Kind: ArticleCreateKind.Variation,
                FromArticleGuid: fromArticleGuid));

        await fixture.CreateVariationHandler.Received(1).Handle(
            Arg.Is<ArticleCreateVariationCommand>(command =>
                command.Name == new Name("Variation") &&
                command.FromArticleGuid == fromArticleGuid),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_InvokePackCreateHandler_WhenKindIsPack()
    {
        var fixture = new Fixture();
        var fromArticleGuid = Guid.NewGuid();

        await fixture.Sut.Create(
            new ArticleCreateDto(
                new Name("Pack"),
                Kind: ArticleCreateKind.Pack,
                FromArticleGuid: fromArticleGuid,
                Quantity: 3.Amount()));

        await fixture.CreatePackHandler.Received(1).Handle(
            Arg.Is<ArticleCreatePackCommand>(command =>
                command.Name == new Name("Pack") &&
                command.FromArticleGuid == fromArticleGuid &&
                command.Quantity.Equals(3.Amount(), 0.Amount())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_InvokeKitCreateHandler_WhenKindIsKit()
    {
        var fixture = new Fixture();
        var pack = new ArticleCreateKitPackDto(Guid.NewGuid(), 2.Amount());

        await fixture.Sut.Create(
            new ArticleCreateDto(
                new Name("Kit"),
                Kind: ArticleCreateKind.Kit,
                KitPacks: [pack]));

        await fixture.CreateKitHandler.Received(1).Handle(
            Arg.Is<ArticleCreateKitCommand>(command =>
                command.Name == new Name("Kit") &&
                command.Components.Single() == new ArticleKitComponent(pack.ArticleGuid, pack.Quantity)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_InvokeContainerCreateHandler_WhenKindIsContainer()
    {
        var fixture = new Fixture();
        var maxMass = Mass.FromKilograms(10);

        await fixture.Sut.Create(
            new ArticleCreateDto(
                new Name("Container"),
                Kind: ArticleCreateKind.Container,
                ContainerMaxMass: maxMass));

        await fixture.CreateContainerHandler.Received(1).Handle(
            Arg.Is<ArticleCreateContainerCommand>(command =>
                command.Name == new Name("Container")),
            Arg.Any<CancellationToken>());
        await fixture.SetContainerMaxMassHandler.Received(1).Handle(
            Arg.Is<ArticleSetContainerMaxMassCommand>(command =>
                command.MaxMass!.Value.Equals(maxMass, Mass.Zero)),
            Arg.Any<CancellationToken>());
    }

    private sealed class Fixture
    {
        public ICommandHandler<ArticleCreateCommand, Guid> CreateArticleHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreateCommand, Guid>>();

        public ICommandHandler<ArticleCreateVariationCommand, Guid> CreateVariationHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreateVariationCommand, Guid>>();

        public ICommandHandler<ArticleCreatePackCommand, Guid> CreatePackHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreatePackCommand, Guid>>();

        public ICommandHandler<ArticleCreateKitCommand, Guid> CreateKitHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreateKitCommand, Guid>>();

        public ICommandHandler<ArticleCreateContainerCommand, Guid> CreateContainerHandler { get; } =
            Substitute.For<ICommandHandler<ArticleCreateContainerCommand, Guid>>();

        public ICommandHandler<ArticleSetContainerMaxMassCommand> SetContainerMaxMassHandler { get; } =
            Substitute.For<ICommandHandler<ArticleSetContainerMaxMassCommand>>();

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
            CreateVariationHandler.Handle(Arg.Any<ArticleCreateVariationCommand>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
            CreatePackHandler.Handle(Arg.Any<ArticleCreatePackCommand>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
            CreateKitHandler.Handle(Arg.Any<ArticleCreateKitCommand>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
            CreateContainerHandler.Handle(Arg.Any<ArticleCreateContainerCommand>(), Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());

            Sut = new ArticleApplicationService(
                CreateArticleHandler,
                CreateVariationHandler,
                CreatePackHandler,
                CreateKitHandler,
                CreateContainerHandler,
                SetContainerMaxMassHandler,
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
