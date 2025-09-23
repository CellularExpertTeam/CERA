using CommunityToolkit.Mvvm.ComponentModel;
using Defencev1.Services.EsriMap;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Extensions.Logging;
using Mapping = Esri.ArcGISRuntime.Mapping;

namespace Defencev1.Controllers.MapController;

public partial class MapController : ObservableObject, IMapController
{
    private readonly ILogger<MapController> _logger;
    private readonly IMapService _mapService;

    [ObservableProperty]
    private Mapping.Map _activeMap = new();

    public MapController(
        ILogger<MapController> logger,
        IMapService mapService,
        BasemapStyle? initial = null)
    {
        _mapService = mapService;
        if (initial != null)
            ActiveMap = new Mapping.Map((BasemapStyle)initial);
        else
            ActiveMap = new Mapping.Map();

        _logger = logger;
    }
    
    public void SetBasemap(BasemapStyle style) => ActiveMap.Basemap = new Basemap(style);
    public void SetBasemap(Basemap basemap) => ActiveMap.Basemap = basemap;

    /// <summary>
    /// Add layer to ActiveMap.OperationalLayers
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="insertAt">For features that have to be above any raster use index starting from 100, othervise use 0</param>
    /// <param name="cloneIfOwned"></param>
    public void AddLayer(Layer layer, int? insertAt = null, bool cloneIfOwned = true)
    {
        // A Layer instance can only belong to one Map/Scene at a time.
        // If it’s already owned elsewhere, clone it before adding.
        var toAdd = layer;

        if (cloneIfOwned)
        {
            try
            {
                var currentLayerCount = ActiveMap.OperationalLayers.Count;
                _logger.LogInformation("Current map layer count: {currentLayerCount}", currentLayerCount);
                if (insertAt.HasValue)
                    ActiveMap.OperationalLayers.Insert(insertAt.Value, toAdd);
                else
                    ActiveMap.OperationalLayers.Add(toAdd);
                return;
            }
            catch (ArcGISRuntimeException e)
            {
                _logger.LogError(e.Message);
                toAdd = (Layer)layer.Clone();
            }
        }

        if (insertAt.HasValue)
            ActiveMap.OperationalLayers.Insert(insertAt.Value, toAdd);
        else
            ActiveMap.OperationalLayers.Add(toAdd);
    }

    public bool RemoveLayerById(string id)
    {
        var layer = GetLayer(id);
        return layer != null && ActiveMap.OperationalLayers.Remove(layer);
    }

    public bool RemoveLayer(Layer layer)
    {
        try
        {
            ActiveMap.OperationalLayers.Remove(layer);
            return true;
        }
        catch (ArcGISRuntimeException e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }

    public void RemoveQuickRFLayers()
    {

        var layersToRemove = new List<Layer>();

        foreach (var layer in ActiveMap.OperationalLayers)
        {
            if (layer.Name.Contains("cell_0_fs1.tif"))
            {
                layersToRemove.Add(layer);
            }
        }

        foreach (var layer in layersToRemove)
        {
            ActiveMap.OperationalLayers.Remove(layer);
        }
    }

    public Layer? GetLayer(string id) => ActiveMap.OperationalLayers[id];

    public void MoveLayer(string id, int newIndex)
    {
        var layer = GetLayer(id);
        if (layer == null) return;
        var old = ActiveMap.OperationalLayers.IndexOf(layer);
        if (old == newIndex) return;
        ActiveMap.OperationalLayers.RemoveAt(old);
        ActiveMap.OperationalLayers.Insert(newIndex, layer);
    }

    public void Reset(Basemap? basemap = null)
    {
        ActiveMap.Bookmarks?.Clear();
        ActiveMap.InitialViewpoint = null;
        List<Layer> layersToRemove = [];
        if (ActiveMap.OperationalLayers.Count > 0)
        {
            foreach (var layer in ActiveMap.OperationalLayers)
            {
                if (!layer.Name.Contains("Basemap"))
                    layersToRemove.Add(layer);
            }

            foreach (var l in layersToRemove)
                ActiveMap.OperationalLayers.Remove(l);
        }

        if (basemap != null)
            ActiveMap.Basemap = basemap;
    }


    public async Task LoadMmpkMap(string mmpkFileName)
    {
        var result = await _mapService.LoadMmpk(Path.Combine(AppContext.BaseDirectory, "Data", mmpkFileName));
        if (result.IsSuccess)
        {
            ActiveMap = result.Value;
            await ActiveMap.LoadAsync();
        }
        else
        {
            _logger.LogError("Error: {result.Error}", result.Error);
        }
    }
}
