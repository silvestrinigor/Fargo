namespace Fargo.Sdk;

/// <summary>
/// Represents the result of an SDK operation.
/// </summary>
/// <remarks>
/// Every client method returns a <see cref="FargoSdkResponse{TData}"/> instead of throwing
/// on API errors. Check <see cref="IsSuccess"/> before accessing <see cref="Data"/>.
/// </remarks>
/// <typeparam name="TData">The type of data returned on success.</typeparam>
public sealed class FargoSdkResponse<TData>
{
    internal FargoSdkResponse()
    {
    }

    internal FargoSdkResponse(TData data)
    {
        Data = data;
    }

    internal FargoSdkResponse(FargoSdkError error)
    {
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// When <see langword="true"/>, <see cref="Data"/> contains the result.
    /// When <see langword="false"/>, <see cref="Error"/> describes the failure.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets the data returned by the operation, or <see langword="null"/> if the operation failed
    /// or returned no content.
    /// </summary>
    public TData? Data { get; }

    /// <summary>
    /// Gets the error returned by the operation, or <see langword="null"/> if the operation succeeded.
    /// </summary>
    public FargoSdkError? Error { get; }
}
