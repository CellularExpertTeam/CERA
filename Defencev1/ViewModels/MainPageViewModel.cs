using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Controllers.MapController;
using Defencev1.Enums;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.ViewModels.Base;
using Defencev1.ViewModels.Predictions;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.Extensions.Logging;

namespace Defencev1.ViewModels;

public partial class MainPageViewModel : ViewModelBase
{
    private readonly ILogger<MainPageViewModel> _logger;
    public IMapController MapController { get; }
    public QuickPredictionViewModel QuickPredictionVM { get; }
    public ProfilePaneViewModel ProfilePaneVM { get; }
    public ProfileGraphViewModel ProfileGraphVM { get; }
    public WorkspaceViewModel WorkspaceVM { get; }
    public LayerViewModel LayerVM { get; }
    public SettingsViewModel SettingsVM { get; }
    public UserProfileViewModel UserProfileVM { get; }
    public FilesPaneViewModel FilesVM { get; }

    [ObservableProperty]
    private MapPoint _pointCoordinates;

    [ObservableProperty]
    private UserProfile _selectedUserProfile;

    partial void OnPointCoordinatesChanged(MapPoint value)
    {
        WeakReferenceMessenger.Default.Send(new MapPointPositionChangedMsg(value));
    }

    [ObservableProperty]
    private Tools _selectedTool = Tools.None;

    [ObservableProperty]
    private GraphicsOverlayCollection _graphicsOverlays = [];

    public MainPageViewModel(
        ILogger<MainPageViewModel> logger,
        IMapController mapController,
        QuickPredictionViewModel quickPredictionVm,
        ProfilePaneViewModel profilePaneVM,
        ProfileGraphViewModel profileGraphVM,
        SettingsViewModel settingsVM,
        WorkspaceViewModel workspaceVM,
        UserProfileViewModel userProfileViewModel,
        FilesPaneViewModel filesVM,
        LayerViewModel layerVM)
    {
        _logger = logger;
        MapController = mapController;
        QuickPredictionVM = quickPredictionVm;
        ProfilePaneVM = profilePaneVM;
        UserProfileVM = userProfileViewModel;

        WeakReferenceMessenger.Default.Register<ControlPanelToolSelectedMsg>(this, (r, m) =>
        {
            SelectedTool = m.Value;
        });

        WeakReferenceMessenger.Default.Register<UserProfileChangedMsg>(this,async (r, m) => await HandleNewLogin(m.Value));

        ProfileGraphVM = profileGraphVM;
        SettingsVM = settingsVM;
        WorkspaceVM = workspaceVM;
        FilesVM = filesVM;
        LayerVM = layerVM;
    }

    public async Task HandleNewLogin(UserProfile user)
    {
        SelectedUserProfile = user;
        WorkspaceVM.Reset();

        try
        {
            var basemap = new Basemap(BasemapStyle.ArcGISImageryStandard);
            if (basemap is not null)
                MapController.Reset(basemap);
        }
        catch (Exception e)
        {
            _logger.LogError("{e.Message}", e.Message);
        }
    }
}
