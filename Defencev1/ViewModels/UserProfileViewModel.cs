using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.Services.Workspaces;
using Defencev1.ViewModels.Base;
using Esri.ArcGISRuntime.Security;
using Microsoft.Extensions.Logging;

namespace Defencev1.ViewModels;

public partial class UserProfileViewModel : ViewModelBase
{
    [ObservableProperty]
    private UserProfile _currentUserProfile = new();

    [ObservableProperty]
    private string _operatingModeIndicator = string.Empty;
    private readonly ILogger<UserProfileViewModel> _logger;
    private readonly IWorkspaceService _workspaceService;

    public UserProfileViewModel(IWorkspaceService workspaceSerive, ILogger<UserProfileViewModel> logger)
    {
        _logger = logger;
        _workspaceService = workspaceSerive;
        WeakReferenceMessenger.Default.Register<UserProfileChangedMsg>(this, (r, m) =>
        {
            CurrentUserProfile = m.Value;
        });
    }

    [RelayCommand]
    public async Task SignOut()
    {
        try
        {
            SecureStorage.Remove("CE_TOKEN");
            AuthenticationManager.Current.RemoveAllCredentials();
            await Shell.Current.GoToAsync("//loginpage");
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {e.Message}", e.Message);
        }
    }

}