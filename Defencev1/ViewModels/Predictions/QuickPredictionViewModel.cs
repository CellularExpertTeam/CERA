using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Controllers.MapController;
using Defencev1.Messages;
using Defencev1.Models;
using Defencev1.Services.Layers;
using Defencev1.Services.PredictionModels;
using Defencev1.Services.Predictions;
using Defencev1.Services.Workspaces;
using Defencev1.Utils;
using Defencev1.ViewModels.Base;
using Esri.ArcGISRuntime.Geometry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Defencev1.ViewModels.Predictions;

public partial class QuickPredictionViewModel : ViewModelBase
{
    private readonly IQuickPredictionService _quickPredictionService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IMapController _mapController;
    private readonly IPredictionModelService _predictionModelService;
    private readonly ILogger<QuickPredictionViewModel> _logger;
    public QuickPredictionViewModel(
       IQuickPredictionService quickPredictionService,
       IWorkspaceService workspaceService,
       IMapController mapController,
       IPredictionModelService predictionModelService,
       ILogger<QuickPredictionViewModel> logger)
    {
        _quickPredictionService = quickPredictionService;
        _workspaceService = workspaceService;
        _mapController = mapController;
        _predictionModelService = predictionModelService;
        _logger = logger;
        WeakReferenceMessenger.Default.Register<MapPointPositionChangedMsg>(this, (r, m) =>
        {
            MapPointPosition = m.Value;
        });
    }

    [ObservableProperty]
    private MapPoint _mapPointPosition;

    [ObservableProperty]
    private ObservableCollection<CE_Cell> _cells;

    [ObservableProperty]
    private ObservableCollection<PredictionModel> _predictionModels;

    [ObservableProperty]
    private PredictionModel? _selectedPredictionModel;

    [ObservableProperty]
    private uint _resolution = 10;

    [ObservableProperty]
    private string _predictionResultMsg = string.Empty;

    [ObservableProperty]
    private double _height = 1;

    [ObservableProperty]
    private double _azimuth = 0;

    [RelayCommand]
    public async Task RunPrediction()
    {
        var workspaceId = _workspaceService?.ActiveWorkspace?.Id;
        if (workspaceId is null)
        {
            PredictionResultMsg = "Active workspace not found";
            return;
        }
        if (MapPointPosition is null)
        {
            PredictionResultMsg = "Select a point on the map to start prediction";
            return;
        }

        var latLong = CoordinateUtils.GetLatLong(MapPointPosition);
        CE_Cell newCell = new()
        {
            Y = latLong.Item1,
            X = latLong.Item2,
            Height = this.Height,
            Azimuth = this.Azimuth,
        };

        QuickRfRequest request = new()
        {
            Cell = newCell,
            CellSize = (int)Resolution,
        };

        var quickRfResult = await _quickPredictionService.PostQuickPrediction((long)workspaceId, request);

        if (!quickRfResult.IsSuccess)
        {
            PredictionResultMsg = quickRfResult.Error;
            return;
        }

        var response = quickRfResult.Value;
        var obj = JObject.Parse(response);
        string resultRasterFileName = string.Empty;
        try
        {
            resultRasterFileName = (string)obj["data"]?["result_list"]?[0]?["fileName"];
        }
        catch (Exception e)
        {
            Debug.Write(e);
            resultRasterFileName = "cell_0_fs1.tif";
        }

        if(!long.TryParse((string)obj["data"]?["object_id"], out var taskId))
        {
            PredictionResultMsg = "Failed to retrieve task id";
            return;
        }

        if (string.IsNullOrEmpty(resultRasterFileName))
        {
            PredictionResultMsg = "Failed to retrieve prediciton results";
            return;
        }

        var rasterResponse = await _quickPredictionService.GetPredictionRaster((long)workspaceId, taskId, resultRasterFileName);

        if (!rasterResponse.IsSuccess)
        {
            PredictionResultMsg = "Failed to extract raster file.";
            return;
        }

        try
        {
            _mapController.RemoveQuickRFLayers();
            var qrfLayer = await LayerService.CreateQuickPredictionLayer(rasterResponse.Value);
            _mapController.AddLayer(qrfLayer);
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {e.Message}", e.Message);
        }
        PredictionResultMsg = "Successfully ran quick prediction.";
    }

    public async Task GetPredictionModels()
    {
        try
        {
            var predictionModelResponse = await _predictionModelService.GetPredictionModels();
            if (!predictionModelResponse.IsSuccess)
            {
                PredictionResultMsg += "Could not get prediction models. ";
                return;
            }

            PredictionModels = new ObservableCollection<PredictionModel>(predictionModelResponse.Value.Data.Values);
            SelectedPredictionModel = PredictionModels.FirstOrDefault();
        }
        catch (Exception e)
        {
            Debug.Write(e.Message);
        }
    }
}
