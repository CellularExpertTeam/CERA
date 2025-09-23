using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Enums;
using Defencev1.Messages;
using Defencev1.Utils.Extensions;
using Defencev1.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Defencev1.Models;

namespace Defencev1.Views;

public partial class MainPage
{
    private readonly MainPageViewModel _mainPageVM;
    public bool ProfileDrawingActivated { get; set; }

    private GraphicsOverlay _lineOverlay = new();
    private GraphicsOverlay _pointOverlay = new();
    private GraphicsOverlay _workspaceBoundOverlay = new();
    public MapPoint[] linePoints = new MapPoint[2];

    SimpleMarkerSymbol pointSymbol = new()
    {
        Style = SimpleMarkerSymbolStyle.Circle,
        Color = System.Drawing.Color.Red,
        Size = 10.0
    };

    SimpleMarkerSymbol transmitterSymbol = new()
    {
        Style = SimpleMarkerSymbolStyle.Circle,
        Color = System.Drawing.Color.LimeGreen,
        Size = 10.0
    };

    SimpleMarkerSymbol receiverSymbol = new()
    {
        Style = SimpleMarkerSymbolStyle.Circle,
        Color = System.Drawing.Color.Magenta,
        Size = 10.0
    };
    public MainPage(MainPageViewModel mainPageViewModel)
    {
        InitializeComponent();

        _mainPageVM = mainPageViewModel;
        this.BindingContext = _mainPageVM;
        _mainPageVM.GraphicsOverlays.Add(_lineOverlay);
        _mainPageVM.GraphicsOverlays.Add(_pointOverlay);
        _mainPageVM.GraphicsOverlays.Add(_workspaceBoundOverlay);

        EsriMap.GeoViewTapped += OnMapViewTapped;
        EsriMap.GeometryEditor = new();
        WeakReferenceMessenger.Default.Register<ControlPanelToolSelectedMsg>(this, (_, __) => ToolsToDefaultBackground());
        WeakReferenceMessenger.Default.Register<NewWorkspaceOpenedMsg>(this, async (r, m) => await ZoomToWorkspace(m.Value));
        WeakReferenceMessenger.Default.Register<UserProfileChangedMsg>(this, (_, __) => ClearGraphicOverlays());
    }

    private async void OnMapViewTapped(object? sender, GeoViewInputEventArgs e)
    {
        var IdentifiedLayers = await EsriMap.IdentifyLayersAsync(e.Position, 10, false, 10);
        var quick_rf_layer = IdentifiedLayers.Where(l => l.LayerContent.Name.Contains("cell_0_fs1")).FirstOrDefault();

        _mainPageVM.PointCoordinates = e.Location;

        if (_mainPageVM.SelectedTool == Tools.QuickPrediction)
        {
            // Run QuickPrediction
            _pointOverlay.Graphics.Clear();
            _pointOverlay.Graphics.Add(new Graphic(e.Location, pointSymbol));
            await _mainPageVM.QuickPredictionVM.RunPrediction();
        }
        else if (_mainPageVM.SelectedTool == Tools.Profile)
        {
            // Draw a line
            if (linePoints.Where(i => i is not null).Count() == 0)
            {
                linePoints[0] = e.Location;
                _lineOverlay.Graphics.Clear();
                _lineOverlay.Graphics.Add(new Graphic(e.Location, transmitterSymbol));
            }
            else if (linePoints.Where(i => i is not null).Count() == 1)
            {
                linePoints[1] = e.Location;

                DrawLine();
                _mainPageVM.ProfilePaneVM.CalculateProfileCommand.Execute(null);
                Array.Clear(linePoints, 0, linePoints.Length);
                _lineOverlay.Graphics.Add(new Graphic(e.Location, receiverSymbol));
            }
        }
        else
        {
            // Add a single point
            _pointOverlay.Graphics.Clear();
            _pointOverlay.Graphics.Add(new Graphic(e.Location, pointSymbol));
        }
    }

    private async void DrawLine()
    {
        var line = new Polyline(linePoints);
        var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 3.0);
        var polylineGraphic = new Graphic(line, lineSymbol);
        _lineOverlay.Graphics.Add(polylineGraphic);
        _mainPageVM.ProfilePaneVM.Transmitter.X = linePoints[0].ToLongitude();
        _mainPageVM.ProfilePaneVM.Transmitter.Y = linePoints[0].ToLatitude();
        _mainPageVM.ProfilePaneVM.Receiver.X = linePoints[1].ToLongitude();
        _mainPageVM.ProfilePaneVM.Receiver.Y = linePoints[1].ToLatitude();
    }

    private void OnOptionClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button)
        {
            string optionName = button.CommandParameter as string ?? "hide";
            WeakReferenceMessenger.Default.Send(new ControlPanelOptionSelectedMsg(optionName));
        }
    }

    private void OnBasemapButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button)
            BasemapGallery.IsVisible = !BasemapGallery.IsVisible;
    }

    private void OnToolClicked(object sender, EventArgs e)
    {

        if (sender is ImageButton button)
        {
            Color activatedBg = Color.FromArgb("#4a4a4a");
            if (App.Current.Resources.TryGetValue("Background2", out object c2))
            {
                activatedBg = (Color)c2;
            }

            Color defaultBg = Color.FromArgb("#2c2c2c");
            if (App.Current.Resources.TryGetValue("Background", out object c))
            {
                defaultBg = (Color)c;
            }

            QuickPredictTool.BackgroundColor = defaultBg;
            ProfileTool.BackgroundColor = defaultBg;

            Tools selectedTool = button.CommandParameter switch
            {
                "profile" => Tools.Profile,
                "quick_predict" => Tools.QuickPrediction,
                _ => Tools.None,
            };

            WeakReferenceMessenger.Default.Send(new ControlPanelToolSelectedMsg(selectedTool));

            if (selectedTool == Tools.Profile)
            {
                _mainPageVM.SelectedTool = Tools.Profile;
                ProfileTool.BackgroundColor = activatedBg;
            }
            else if (selectedTool == Tools.QuickPrediction)
            {
                _mainPageVM.SelectedTool = Tools.QuickPrediction;
                QuickPredictTool.BackgroundColor = activatedBg;
            }
            else
            {
                _mainPageVM.SelectedTool = Tools.None;
            }
        }
    }

    private void ToolsToDefaultBackground()
    {
        Color defaultBg = Color.FromArgb("#2c2c2c");
        if (App.Current.Resources.TryGetValue("Background", out object c))
        {
            defaultBg = (Color)c;
        }

        QuickPredictTool.BackgroundColor = defaultBg;
        ProfileTool.BackgroundColor = defaultBg;
    }

    private async void UserProfileButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            UserProfileView.IsVisible = !UserProfileView.IsVisible;
        }
    }

    private async Task ZoomToWorkspace(Workspace workspace)
    {
        _workspaceBoundOverlay.Graphics.Clear();
        SpatialReference sp = SpatialReference.Create(workspace.Epsg);
        Envelope extent = new(
            workspace.ExtentXmin,
            workspace.ExtentYmin,
            workspace.ExtentXmax,
            workspace.ExtentYmax,
            sp);

        if (extent != null)
        {
            await EsriMap.SetViewpointGeometryAsync(extent, 50);

            var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Blue, 3);
            MapPoint a = new(workspace.ExtentXmin, workspace.ExtentYmin);
            MapPoint b = new(workspace.ExtentXmin, workspace.ExtentYmax);
            MapPoint c = new(workspace.ExtentXmax, workspace.ExtentYmax);
            MapPoint d = new(workspace.ExtentXmax, workspace.ExtentYmin);

            var border = new Polygon([a, b, c, d], sp);
            var borderGraphic = new Graphic(border, lineSymbol);
            _workspaceBoundOverlay.Graphics.Add(borderGraphic);
        }
    }

    private void ClearGraphicOverlays()
    {
        _workspaceBoundOverlay.Graphics.Clear();
        _lineOverlay.Graphics.Clear();
        _pointOverlay.Graphics.Clear();
    }
}
