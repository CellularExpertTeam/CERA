using Defencev1.Controllers.MapController;
using Defencev1.ViewModels.Base;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Extensions.Logging;

namespace Defencev1.ViewModels;

public partial class LayerViewModel : ViewModelBase
{
    private readonly ILogger<LayerViewModel> _logger;
    public IMapController MapController { get; }
    public LayerViewModel(IMapController mapController, ILogger<LayerViewModel> logger)
    {
        MapController = mapController;
        _logger = logger;
    }

    public void RemoveLayer(Layer layer)
    {
        if (layer == null)
            return;

        try
        {
            var result = MapController.RemoveLayer(layer);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to remove layer {layer?.Name}: {e.Message}", layer?.Name, e.Message);
        }
    }
}
