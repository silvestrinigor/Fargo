using Fargo.Sdk;

namespace Fargo.Web.Components.Shared;

public sealed class AsyncState
{
    public bool IsBusy { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<bool> RunAsync(Func<Task> action, string fallbackError)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await action();
            return true;
        }
        catch (FargoSdkException ex)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message) ? fallbackError : ex.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Clear()
    {
        IsBusy = false;
        ErrorMessage = null;
    }
}
