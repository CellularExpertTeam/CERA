using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Models.ProfileModels;
using Defencev1.Services.Profile;
using Defencev1.Services.Workspaces;
using Defencev1.ViewModels.Base;
using Defencev1.ViewModels.Predictions;

namespace Defencev1.ViewModels;

public partial class ProfilePaneViewModel : ViewModelBase
{
    private readonly IProfileService _profileService;
    public ProfileGraphViewModel ProfileGraphViewModel;
    private readonly IWorkspaceService _workspaceService;
    public ProfilePaneViewModel(IProfileService profileService, IWorkspaceService workspaceService, ProfileGraphViewModel profileGraphVm)
    {
        _profileService = profileService;
        _workspaceService = workspaceService;
        ProfileGraphViewModel = profileGraphVm;
    }
    [ObservableProperty]
    private bool _profileToolEnabled = false;
    partial void OnProfileToolEnabledChanged(bool oldValue, bool newValue)
    {
        WeakReferenceMessenger.Default.Send(new Messages.ProfileToolStatusMsg(newValue));
    }

    [ObservableProperty]
    private Transmitter _transmitter = new();

    [ObservableProperty]
    private Receiver _receiver = new();

    [ObservableProperty]
    private string _profileCalculationResultMsg = string.Empty;

    [ObservableProperty]
    private ProfileResponse? _currentProfile;

    [RelayCommand]
    public async Task CalculateProfile()
    {
        var workspaceId = _workspaceService.ActiveWorkspace?.Id;
        if (workspaceId is null)
        {
            ProfileCalculationResultMsg = "No active workspace";
            return;
        }
        ProfileRequest profile = new() { Receiver = Receiver, Transmitter = Transmitter };
        var validation = ProfileService.Validate(profile);
        if (!validation.IsSuccess)
        {
            ProfileCalculationResultMsg = "Invalide profile parameters";
            return;
        }
        var response = await _profileService.PostProfile(_workspaceService.ActiveWorkspace.Id, profile);
        if (!response.IsSuccess)
        {
            ProfileCalculationResultMsg = $"Profile calculation failed: {response.Error}";
            return;
        }
        CurrentProfile = response.Value;

        WeakReferenceMessenger.Default.Send(new Messages.CalculateProfileMsg(true));
    }
}
