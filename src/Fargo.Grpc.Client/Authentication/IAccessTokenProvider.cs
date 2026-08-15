public interface IAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(
        CancellationToken cancellationToken = default);
}
