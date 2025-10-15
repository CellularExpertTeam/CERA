using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Enums;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.Services.Auth;
using Defencev1.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace Defencev1.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private Credentials _credentials = new();

    [ObservableProperty]
    private AppOperatingMode _selectedOperatingMode = AppOperatingMode.LocalServices;

    [ObservableProperty]
    private string _resultMsg = string.Empty;

    [ObservableProperty]
    private bool _saveUserCredentials = false;

    public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
    {
        _logger = logger;
        _authService = authService;
        _ = IsBusyFor(InitializeAsync);
    }

    public override async Task InitializeAsync()
    {
        await SwitchProfile();
        await CheckLoginStatus();
    }

    [RelayCommand]
    private async Task Login()
    {
        ResultMsg = string.Empty;

        try
        {
            string configUrl = $"{Credentials.CeServerUrl}/ce_express_react/config.json";
            var configResult = await _authService.GetConfig(configUrl);
            if (!configResult.IsSuccess)
            {
                ResultMsg = configResult.Error;
                return;
            }

            Credentials.ApiUrl = configResult.Value.CeApiUrl.Trim('/');
            Credentials.PortalUrl = configResult.Value.PortalUrl.Trim('/');

            switch (SelectedOperatingMode)
            {
                case AppOperatingMode.OnlineServices:

                    var onlineToken = await _authService.GenerateCEToken(Credentials);
                    if (onlineToken.IsSuccess)
                    {
                        Preferences.Set("APP_OPERATING_MODE", AppOperatingMode.OnlineServices.ToString());
                        ResultMsg = onlineToken.Value;

                        var token = await SecureStorage.GetAsync("CE_TOKEN");

                        UserProfile userProfile = new ();
                        userProfile.SavedOperatingMode = AppOperatingMode.OnlineServices;
                        userProfile.UserCredentials = Credentials;
                        userProfile.CEToken = token;
                        if (SaveUserCredentials)
                            await userProfile.Save();
                        await Shell.Current.GoToAsync("//mainpage");

                        WeakReferenceMessenger.Default.Send(new UserProfileChangedMsg(userProfile));
                    }
                    else
                    {
                        ResultMsg = onlineToken.Error;
                    }
                    break;

                case AppOperatingMode.LocalServices:
                    var localToken = await _authService.GenerateCEToken(Credentials);
                    if (localToken.IsSuccess)
                    {
                        Preferences.Set("APP_OPERATING_MODE", AppOperatingMode.LocalServices.ToString());
                        ResultMsg = localToken.Value;

                        var token = await SecureStorage.GetAsync("CE_TOKEN");

                        UserProfile userProfile = new();
                        userProfile.SavedOperatingMode = AppOperatingMode.LocalServices;
                        userProfile.UserCredentials = Credentials;
                        userProfile.CEToken = token;
                        if (SaveUserCredentials)
                            await userProfile.Save();
                        await Shell.Current.GoToAsync("//mainpage");

                        WeakReferenceMessenger.Default.Send(new UserProfileChangedMsg(userProfile));
                    }
                    else
                    {
                        ResultMsg = localToken.Error;
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            ResultMsg = $"An error occurred: {e.Message}";
        }
    }

    [RelayCommand]
    public void Cancel()
    {
        Credentials = new();
    }

    [RelayCommand]
    public async Task SetAppOperatingMode(string mode)
    {
        SelectedOperatingMode = Enum.Parse<AppOperatingMode>(mode);
        await SwitchProfile();
    }

    private async Task CheckLoginStatus()
    {
        SecureStorage.Remove("CE_TOKEN");
        bool isLoggedIn = await IsUserLoggedInAsync();

        if (isLoggedIn)
        {
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//mainpage"));
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//loginpage"));
        }
    }

    private async Task<bool> IsUserLoggedInAsync()
    {
        string? authToken = await SecureStorage.GetAsync("CE_TOKEN");
        return !string.IsNullOrEmpty(authToken);
    }

    public async Task SwitchProfile()
    {
        if (SelectedOperatingMode is AppOperatingMode.OnlineServices)
        {
            Credentials.UserName = await SecureStorage.GetAsync("USERNAME_ONLINE");
            Credentials.Password = await SecureStorage.GetAsync("PASSWORD_ONLINE");
            Credentials.CeServerUrl = await SecureStorage.GetAsync("CE_SERVER_URL_ONLINE");
        }
        else if (SelectedOperatingMode is AppOperatingMode.LocalServices)
        {
            Credentials.UserName = await SecureStorage.GetAsync("USERNAME_LOCAL");
            Credentials.Password = await SecureStorage.GetAsync("PASSWORD_LOCAL");
            Credentials.CeServerUrl = await SecureStorage.GetAsync("CE_SERVER_URL_LOCAL");
        }
    }
}
