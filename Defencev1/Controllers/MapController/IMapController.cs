using Esri.ArcGISRuntime.Mapping;
using Mapping = Esri.ArcGISRuntime.Mapping;

namespace Defencev1.Controllers.MapController;

public interface IMapController
{
    Mapping.Map ActiveMap { get; }

    void SetBasemap(Mapping.BasemapStyle style);
    void SetBasemap(Mapping.Basemap basemap);
    void AddLayer(Mapping.Layer layer, int? insertAt = null, bool cloneIfOwned = true);
    bool RemoveLayerById(string id);
    bool RemoveLayer(Layer layer);
    public void RemoveQuickRFLayers();
    Mapping.Layer? GetLayer(string id);
    void MoveLayer(string id, int newIndex);
    void Reset(Basemap? basemap = null);
    Task LoadMmpkMap(string mmpkFileName);
}
