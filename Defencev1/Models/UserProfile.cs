using Defencev1.Enums;

namespace Defencev1.Models;

public record UserProfile
{
    public AppOperatingMode SavedOperatingMode { get; set; }
    public Credentials UserCredentials { get; set; }
    public string CEToken { get; set; }

    public async Task Save()
    {
        switch (SavedOperatingMode)
        {
            case AppOperatingMode.OnlineServices:
                await SecureStorage.SetAsync("CE_SERVER_URL_ONLINE", UserCredentials.CeServerUrl);
                await SecureStorage.SetAsync("USERNAME_ONLINE", UserCredentials.UserName);
                await SecureStorage.SetAsync("PASSWORD_ONLINE", UserCredentials.Password);
                break;
            case AppOperatingMode.LocalServices:
                await SecureStorage.SetAsync("CE_SERVER_URL_LOCAL", UserCredentials.CeServerUrl);
                await SecureStorage.SetAsync("USERNAME_LOCAL", UserCredentials.UserName);
                await SecureStorage.SetAsync("PASSWORD_LOCAL", UserCredentials.Password);
                break;
            default:
                break;
        }
    }
}
