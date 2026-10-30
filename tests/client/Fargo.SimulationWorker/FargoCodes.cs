namespace Fargo.SimulationWorker;

/// <summary>
/// The server enum codes exposed by the API as plain integers. These mirror the
/// Fargo.Core enums; the generated client transmits them numerically.
/// </summary>
internal static class FargoCodes
{
    // ArticleType (Fargo.Core.Articles.ArticleType).
    public const int ArticleTypeDefault = 1;
    public const int ArticleTypeContainer = 5;

    // ArticleTrackingMode (Fargo.Core.Articles.ArticleTrackingMode).
    public const int TrackingEntity = 1;
    public const int TrackingEvent = 2;

    // ItemEventType (Fargo.Core.Items.Events.ItemEventType).
    public const int EventMoviment = 1;
    public const int EventCreation = 2;
    public const int EventDeletion = 3;

    // ActionType permission codes (Fargo.Core.Actors.ActionType).
    public const int CreateArticle = 1;
    public const int DeleteArticle = 2;
    public const int EditArticle = 3;
    public const int CreateItem = 4;
    public const int DeleteItem = 5;
    public const int EditItem = 6;
    public const int ReadAuditLogs = 18;
    public const int ReadItemEvents = 19;
}
