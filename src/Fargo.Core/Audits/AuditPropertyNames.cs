namespace Fargo.Core.Audits;

public static class AuditPropertyNames
{
    public static class ArticleCreated
    {
        public const string ArticleName = "name";

        public const string ArticleType = "articleType";

        public const string ArticleDescription = "description";

        public const string ArticleShelfLife = "shelfLife";

        public const string ArticleVariation = "variation";

        public const string ArticleVariationFromArticleGuid = "fromArticleGuid";

        public const string ArticlePack = "pack";

        public const string ArticlePackFromArticleGuid = "fromArticleGuid";

        public const string ArticlePackQuantity = "quantity";
    }
}
