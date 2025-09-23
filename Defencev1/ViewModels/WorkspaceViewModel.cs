using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Controllers.MapController;
using Defencev1.Enums;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.Services.Workspaces;
using Defencev1.ViewModels.Base;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Defencev1.ViewModels;

public partial class WorkspaceViewModel(IWorkspaceService workspaceService, IMapController mapController) : ViewModelBase
{
    private readonly IWorkspaceService _workspaceService = workspaceService;
    public IMapController MapController { get; } = mapController;

    [ObservableProperty]
    private string _workspaceResultMsg = string.Empty;

    [ObservableProperty]
    private Color _workspaceResultMsgColor = Colors.White;

    [ObservableProperty]
    private Workspace? _selectedWorkspace;
    partial void OnSelectedWorkspaceChanged(Workspace value)
    {
        if (value != null && value.Name == ActiveWorkspace?.Name)
            return;
        WorkspaceResultMsg = string.Empty;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await GetActiveWorkspaceCommand.ExecuteAsync(null);
        });
    }

    [ObservableProperty]
    private ObservableCollection<Workspace> _workspaces = new();

    [ObservableProperty]
    private Workspace? _activeWorkspace;

    [RelayCommand]
    private async Task GetActiveWorkspace()
    {
        try
        {
            if (SelectedWorkspace?.Id is null)
            {
                return;
            }
            var result = await _workspaceService.GetWorkspaceById(SelectedWorkspace.Id);
            if (!result.IsSuccess)
            {
                WorkspaceResultMsg = "Failed to get workspace.";
                WorkspaceResultMsgColor = Colors.Red;
                return;
            }
            else
            {
                WorkspaceResultMsg = result.Value;
                WorkspaceResultMsgColor = Colors.LimeGreen;
            }

            ActiveWorkspace = _workspaceService.ActiveWorkspace;
            await AddNewWorkspaceLayers();
            WeakReferenceMessenger.Default.Send(new NewWorkspaceOpenedMsg(ActiveWorkspace));
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            WorkspaceResultMsg = "Error retrieving workspace";
            WorkspaceResultMsgColor = Colors.Red;
        }
    }

    [RelayCommand]
    private async Task GetWorkspaces()
    {
        var result = await _workspaceService.GetWorkspaces();
        if (result.IsSuccess)
        {
            var workspaceList = result.Value.OrderBy(w => w.Id);
            Workspaces = new ObservableCollection<Workspace>(workspaceList);
        }
        else
        {
            WorkspaceResultMsg = result.Error;
        }
    }

    public async Task AddNewWorkspaceLayers()
    {
        MapController.Reset();

        try
        {
            List<string> layerUrls = JsonConvert.DeserializeObject<List<string>>(_workspaceService.ActiveWorkspace.ExtraLayers);
            List<Layer> featureLayers = await GetFeatureLayersFromUrls(layerUrls);
            List<Layer> geodatasets = await GetCurrentWorkspaceGeodatasets();

            MapController.ActiveMap.OperationalLayers.AddRange([.. featureLayers, .. geodatasets]);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task<List<Layer>> GetCurrentWorkspaceGeodatasets()
    {
        List<Layer> layers = [];
        var geodatasetsResult = await _workspaceService.GetGeodatasets();
        if (geodatasetsResult.IsSuccess)
        {
            foreach (var layerPath in geodatasetsResult.Value)
            {
                double[] minMax = [0, 300];
                MinMaxStretchParameters minMaxParams = new([minMax.Min()], [minMax.Max()]);
                ColorRamp ramp = ColorRamp.Create(PresetColorRampType.Elevation);
                StretchRenderer renderer = new(minMaxParams, null, true, ramp);

                var raster = new Raster(layerPath);
                var rasterLayer = new RasterLayer(raster);
                await rasterLayer.LoadAsync();
                rasterLayer.IsVisible = false;
                rasterLayer.Renderer = renderer;
                rasterLayer.Opacity = 0.5;

                layers.Add(rasterLayer);
            }
        }
        return layers;
    }

    public async Task<List<Layer>> GetFeatureLayersFromUrls(List<string> urls)
    {
        List<Layer> layers = [];
        foreach (string url in urls)
        {
            int i = 1;
            var layer = await _workspaceService.CreateLayerFromUrl(url);
            if (layer is not null)
            {
                await layer.LoadAsync();
                if (string.IsNullOrWhiteSpace(layer.Name))
                {
                    layer.Name = $"layer_{i}";
                }
                layers.Add(layer);
                ++i;
            }
        }
        return layers;
    }

    public void Reset()
    {
        _workspaceService.ActiveWorkspace = null;
        ActiveWorkspace = null;
        SelectedWorkspace = null;
        WorkspaceResultMsg = string.Empty;
        WorkspaceResultMsgColor = Colors.White;
    }
}
