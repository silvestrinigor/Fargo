namespace Fargo.Sdk.Articles;

/// <summary>
/// Combined article interface: CRUD, image management, barcode management, and Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IArticleManager : IArticleService, IArticleImageService, IArticleBarcodeService, IArticleEventSource { }
