namespace Fargo.HttpClient;

public interface IFargoHttpClient
{
    IFargoIdentityClient Identity { get; }

    IFargoArticleClient Articles { get; }

    IFargoItemClient Items { get; }

    IFargoUserClient Users { get; }

    IFargoUserGroupClient UserGroups { get; }

    IFargoPartitionClient Partitions { get; }
}

public sealed class FargoHttpClient : IFargoHttpClient
{
    public FargoHttpClient(System.Net.Http.HttpClient httpClient)
    {
        var transport = new FargoHttpTransport(httpClient);

        Identity = new FargoIdentityClient(transport);
        Articles = new FargoArticleClient(transport);
        Items = new FargoItemClient(transport);
        Users = new FargoUserClient(transport);
        UserGroups = new FargoUserGroupClient(transport);
        Partitions = new FargoPartitionClient(transport);
    }

    public IFargoIdentityClient Identity { get; }

    public IFargoArticleClient Articles { get; }

    public IFargoItemClient Items { get; }

    public IFargoUserClient Users { get; }

    public IFargoUserGroupClient UserGroups { get; }

    public IFargoPartitionClient Partitions { get; }
}
