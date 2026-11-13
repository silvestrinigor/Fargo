namespace Fargo.Cli.Authentication;

/// <summary>
/// The OS credential store that holds the session token cannot be used, for example because the
/// Secret Service library (<c>libsecret</c>) is not installed on Linux.
/// </summary>
public sealed class CredentialStoreUnavailableException(Exception innerException)
    : Exception(BuildMessage(), innerException)
{
    private static string BuildMessage()
    {
        var hint = OperatingSystem.IsLinux()
            ? "Install libsecret and a Secret Service such as gnome-keyring "
              + "(for example 'sudo apt install libsecret-1-0 gnome-keyring' on Debian or Ubuntu)"
            : OperatingSystem.IsMacOS()
                ? "Make sure the login keychain is available"
                : "Make sure the Windows Credential Manager is available";

        return "The OS credential store that keeps your Fargo session is not available. "
            + hint
            + ", or pass --username and --password to sign in for a single command.";
    }
}
