using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Controllers.MapController;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.Services.Workspaces;
using Defencev1.ViewModels.Base;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

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
        WorkspaceResultMsg = string.Empty;
        WorkspaceResultMsgColor = Colors.White;
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
            List<ExtraLayer> layerUrls = JsonSerializer.Deserialize<List<ExtraLayer>>(_workspaceService.ActiveWorkspace.ExtraLayers);
            List<Layer> featureLayers = await GetFeatureLayersFromUrls(layerUrls);

            MapController.ActiveMap.OperationalLayers.AddRange([.. featureLayers ]);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task<List<Layer>> GetCurrentWorkspaceGeodatasets()
    {
        StringBuilder sb = new("Retrieving geodatasets...\n");
        List<Layer> layers = [];
        WorkspaceResultMsgColor = Colors.White;
        WorkspaceResultMsg = sb.ToString();
        try
        {
            await foreach (var geodataPath in _workspaceService.GetGeodatasets())
            {
                if (string.IsNullOrEmpty(geodataPath) || !Path.Exists(geodataPath)) continue;

                double[] minMax = [0, 300];
                MinMaxStretchParameters minMaxParams = new([minMax.Min()], [minMax.Max()]);
                ColorRamp ramp = ColorRamp.Create(PresetColorRampType.Elevation);
                StretchRenderer renderer = new(minMaxParams, null, true, ramp);

                var raster = new Raster(geodataPath);
                var rasterLayer = new RasterLayer(raster);
                await rasterLayer.LoadAsync();
                rasterLayer.IsVisible = true;
                rasterLayer.Renderer = renderer;
                rasterLayer.Opacity = 0.5;

                sb.AppendLine($"Added layer: {rasterLayer.Name}");
                layers.Add(rasterLayer);
                WorkspaceResultMsg = sb.ToString();
            }
        }
        catch (Exception e)
        {
            sb.Append(e.ToString());
            Debug.WriteLine(e);
        }

        WorkspaceResultMsg = sb.ToString();
        return layers;

    }

    public async Task<List<Layer>> GetFeatureLayersFromUrls(List<ExtraLayer> extraLayers)
    {
        List<Layer> layers = [];
        int i = 1;
        foreach (string url in extraLayers.Select(e => e.url))
        {
            try
            {
                var layer = await _workspaceService.CreateLayerFromUrl(url);
                if (layer is null) continue;
                await layer.LoadAsync();

                if (string.IsNullOrEmpty(layer.Name))
                    layer.Name = $"layer_{i++}";

                layers.Add(layer);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading workspace extra layer");
                continue;
            }
        }
        return layers;
    }

    [RelayCommand]
    private async Task DownloadGeodata()
    {
        
        if (_workspaceService.ActiveWorkspace is null)
        {
            WorkspaceResultMsg = "Select a workspace.";
            WorkspaceResultMsgColor = Colors.Red;
        }

        foreach (var layer in await GetCurrentWorkspaceGeodatasets())
        {
            try
            {
                MapController.ActiveMap.OperationalLayers.Add(layer);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

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

public sealed class ExtraLayer
{
    public string url { get; set; } = string.Empty;
    public double minScale { get; set; }
    public double opacity { get; set; }
    public bool visible { get; set; }
    public string? title { get; set; }
}
