using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.Utils;
using Defencev1.Utils.Result;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Extensions.Logging;
using Windows.Web.AtomPub;
using Mapping = Esri.ArcGISRuntime.Mapping;
using Models = Defencev1.Models;

namespace Defencev1.Controllers.MapController;

public partial class MapController : ObservableObject, IMapController
{
    private readonly ILogger<MapController> _logger;
    public bool MmpkLoaded { get; private set; } = false;

    [ObservableProperty]
    private Mapping.Map _activeMap = new();

    public MapController(
        ILogger<MapController> logger,
        BasemapStyle? initial = null)
    {
        if (initial != null)
            ActiveMap = new Mapping.Map((BasemapStyle)initial);
        else
            ActiveMap = new Mapping.Map();
        _logger = logger;

        WeakReferenceMessenger.Default.Register<NewWorkspaceOpenedMsg>(this, (_, __) => MmpkLoaded = false);
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

    public async Task<Result<string>> LoadMmpkMap(string mmpkPath, Models.Workspace workspace)
    {
        try
        {
            var workspaceExtent = workspace.GetExtent();
            if (workspaceExtent is null)
                return Result<string>.Fail("Could not determine workspace extent.");
            
            var mobileMapPackage = new MobileMapPackage(mmpkPath);
            await mobileMapPackage.LoadAsync();

            if (mobileMapPackage?.Maps.Count > 0)
            {
                Mapping.Map map = mobileMapPackage.Maps[0];
                var mmpkExtent = await GetMapExtentAsync(map);
                if (mmpkExtent is null)
                    return Result<string>.Fail("Could not determine map package extent.");
                if (!CoordinateUtils.GeometriesIntersect(workspaceExtent, mmpkExtent, out _))
                    return Result<string>.Fail("Mobile map package area does not intersect Workspace area.");

                ActiveMap = map;

                await ActiveMap.LoadAsync();
                MmpkLoaded = true;
                return Result<string>.Ok("Successfully loaded map package.");
            }
            else
            {
                MmpkLoaded = false;
                return Result<string>.Fail("No maps found in package.");
            }
        }
        catch (Exception e)
        {
            return Result<string>.Fail(e.Message);
        }
    }

    public async Task<Envelope?> GetMapExtentAsync(Mapping.Map map)
    {
        await map.LoadAsync();

        Envelope? agg = null;
        foreach (var layer in map.OperationalLayers)
        {
            await layer.LoadAsync();
            var e = layer.FullExtent;
            if (e != null) agg = agg == null ? e : GeometryEngine.CombineExtents(agg, e);
        }
        return agg;
    }
}
