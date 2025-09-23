using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;

namespace Defencev1.Services.Layers;

public class LayerService
{
    public static async Task<Layer> CreateQuickPredictionLayer(string tifFile)
    {
        double[] minMax = [-200, -20]; // Preliminary values
        MinMaxStretchParameters minMaxParams = new([minMax.Min()], [minMax.Max()]);
        ColorRamp ramp = ColorRamp.Create(PresetColorRampType.Elevation)!;
        StretchRenderer renderer = new(minMaxParams, null, false, ramp);

        var raster = new Raster(tifFile);
        var rasterLayer = new RasterLayer(raster);
        await rasterLayer.LoadAsync();
        rasterLayer.IsVisible = true;
        rasterLayer.Renderer = renderer;
        rasterLayer.Opacity = 0.8;

        return rasterLayer;
    }
}
