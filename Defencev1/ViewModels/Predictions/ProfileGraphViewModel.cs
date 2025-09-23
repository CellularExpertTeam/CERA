using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.Models.ClutterModels;
using Defencev1.Models.ProfileModels;
using Defencev1.Services.Profile;
using Defencev1.ViewModels.Base;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Defencev1.ViewModels.Predictions;

public partial class ProfileGraphViewModel : ViewModelBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileService> _logger;

    public ProfileGraphViewModel(IProfileService profileService, ILogger<ProfileService> logger)
    {
        _profileService = profileService;
        _logger = logger;
        WeakReferenceMessenger.Default.Register<CalculateProfileMsg>(this, async (_, __) => await DrawNewChart());
    }

    [ObservableProperty] private ProfileResponse _profile;

    [ObservableProperty] private ObservableCollection<ISeries> _series = new();
    [ObservableProperty] private Axis[] _xAxes;
    [ObservableProperty] private Axis[] _yAxes;
    public List<Clutter> Clutters = [];

    public async Task DrawNewChart()
    {
        await IsBusyFor(async () =>
        {
            Profile = _profileService.CurrentProfile;
            if (Profile is null || Profile.Data is null)
            {
                _logger.LogError("Profile or profile data is null.");
                return;
            }
            await LoadProfileClutters();
            await RebuildChart();
        });
    }

    public async Task LoadProfileClutters()
    {
        var distinctClutterIds = Profile?.Data?.Arrays?.ClutterClass
            .Distinct().ToList() ?? [];

        Clutters = Profile.Data.ClutterClasses
           .Where(kvp => kvp.Value is not null && distinctClutterIds.Contains(kvp.Value.Id))
           .Select(kvp =>
           {
               kvp.Value.Name = kvp.Key;
               return kvp.Value;
           })
           .ToList();
    }

    private async Task RebuildChart()
    {
        Series.Clear();
        var series = new List<ISeries>();

        Task DrawGraph = Task.Run(() =>
        {
            Arrays arrays = Profile.Data.Arrays;
            if (arrays is null)
                return;

            List<double> distances = [.. arrays?.Distance];
            List<double> elevation = [.. arrays?.Elevation];
            List<double> profile = [..arrays?.Profile];

            if (distances is null || elevation is null || profile is null || distances.Count == 0)
            {
                return;
            }

            List<double> fresnel = [.. arrays.Fresnel];
            List<double> clutterH = [.. arrays.ClutterHeight];
            List<int> classes = [.. arrays.ClutterClass];

            List<double> fresnel60 = new(fresnel);
            for (int i = 0; i < fresnel60.Count; i++)
                fresnel60[i] = profile[i] - fresnel60[i] * 0.6;

            for (int i = 0; i < fresnel.Count; i++)
                fresnel[i] = profile[i] - fresnel[i];
            
            series.AddRange(PaintHighClutters(distances, elevation, clutterH, classes));

            // Elevation
            series.Add(new LineSeries<ObservablePoint>
            {
                Name = "Elevation",
                Values = Points(distances, elevation),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.Gray),
                Fill = new SolidColorPaint(SKColors.Gray)
            });

            // Paint on clutters
            series.AddRange(PaintClutterLine(distances, elevation, classes));

            // Profile line
            series.AddRange(DrawProfileLine(distances, profile, Profile.Data.FirstBObstIndex, Profile.Data.FirstCObstIndex));

            // Fresnel line
            series.Add(new LineSeries<ObservablePoint>
            {
                Name = "Fresnel",
                Values = Points(distances, fresnel),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 2 },
                Fill = null
            });

            // Fresnel 60 line
            series.Add(new LineSeries<ObservablePoint>
            {
                Name = "Fresnel 60%",
                Values = Points(distances, fresnel60),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.LightSkyBlue)
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect(new float[] { 8f, 6f })
                },
                Fill = null
            });

            // Transmitter point
            series.Add(new ScatterSeries<ObservablePoint>
            {
                Name = "Transmitter",
                Values = new[] { new ObservablePoint(distances[0], profile[0]) },
                GeometrySize = 15,
                Fill = new SolidColorPaint(SKColors.LimeGreen),
                Stroke = null
            });

            // Receiver point
            series.Add(new ScatterSeries<ObservablePoint>
            {
                Name = "Receiver",
                Values = new[] { new ObservablePoint(distances[^1], profile[^1]) },
                GeometrySize = 15,
                Fill = new SolidColorPaint(SKColors.Magenta),
                Stroke = null
            });

            var maxX = distances[^1] + 5;
            var maxY = Math.Max(elevation.Max(), profile.Max()) + 20;

            XAxes = new[]
            {
                new Axis {
                    MinLimit = -5,
                    MaxLimit = maxX,
                    Name = "Distance, m",
                    NameTextSize = 16,
                    TextSize= 14,
                    NamePaint = new SolidColorPaint(SKColors.White),
                    LabelsPaint = new SolidColorPaint(SKColors.White),
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    { StrokeThickness = 2 },
                    LabelsDensity = 0.1f,
                }
            };
            YAxes = new[]
            {
                new Axis {
                    MinLimit = elevation.Min() - 5,
                    MaxLimit = maxY,
                    Name = "Elevation, m",
                    NameTextSize = 16,
                    TextSize= 14,
                    NamePaint = new SolidColorPaint(SKColors.White),
                    LabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKFontStyle = new SKFontStyle(1, 14, SKFontStyleSlant.Upright),
                    },
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    { StrokeThickness = 2 },
                    LabelsDensity = 0.1f,}
            };

            Series = new ObservableCollection<ISeries>(series);
        });

        await IsBusyFor(async () => await DrawGraph);
    }

    private List<ISeries> DrawProfileLine(List<double> distance, List<double> profile, int bObsIndex, int cObsIndex)
    {
        var profileLine = new List<ISeries>();

        // Green part: from start to cObsIndex
        var greenDistance = distance.GetRange(0, cObsIndex);
        var greenProfile = profile.GetRange(0, cObsIndex);
        profileLine.Add(new LineSeries<ObservablePoint>
        {
            Name = "Profile",
            Values = Points(greenDistance, greenProfile),
            LineSmoothness = 0,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(SKColors.LimeGreen) { StrokeThickness = 2 },
            Fill = null
        });

        // Yellow part: from cObsIndex to bObsIndex
        if(bObsIndex > cObsIndex)
        {
            var yellowDistance = distance.GetRange(cObsIndex + 1, bObsIndex - cObsIndex);
            var yellowProfile = profile.GetRange(cObsIndex + 1, bObsIndex - cObsIndex);
            profileLine.Add(new LineSeries<ObservablePoint>
            {
                Name = "Profile",
                Values = Points(yellowDistance, yellowProfile),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.Yellow) { StrokeThickness = 2 },
                Fill = null
            });
        }
       
        // Red part: from bObsIndex to end
        var redDistance = distance.GetRange(bObsIndex , distance.Count - bObsIndex);
        var redProfile = profile.GetRange(bObsIndex, profile.Count - bObsIndex);
        profileLine.Add(new LineSeries<ObservablePoint>
        {
            Name = "Profile",
            Values = Points(redDistance, redProfile),
            LineSmoothness = 0,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 },
            Fill = null
        });

        return profileLine;
    }

    private List<ISeries> PaintClutterLine(
       List<double> distance,
       List<double> elevation,
       List<int> clutterClasses)
    {
        var series = new List<ISeries>();
        if (Clutters is null || Clutters.Count == 0)
            return series;

        foreach (var clutter in Clutters)
        {
            List<double?> classH = Enumerable.Repeat<double?>(null, distance.Count).ToList();

            for (int i = 0; i < distance.Count; i++)
            {
                if (clutterClasses[i] == clutter.Id)
                {
                    classH[i] = elevation[i];
                }
            }

            if (!SKColor.TryParse((string)clutter.Color, out var sk))
                sk = SKColors.Gray;

            series.Add(new LineSeries<ObservablePoint>
            {
                Name = clutter.Name,
                Values = PointsAllowNull(distance, classH),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(sk) { StrokeThickness = 4 },
                Fill = null
            });
        }

        return series;
    }

    private List<ISeries> PaintHighClutters(
        List<double> distance,
        List<double> elevation,
        List<double> clutterHeights,
        List<int> clutterClasses)
    {
        var series = new List<ISeries>();
        if (Clutters is null || Clutters.Count == 0)
            return series;

        foreach (var clutter in Clutters)
        {
            List<double?> classH = Enumerable.Repeat<double?>(null, distance.Count).ToList();

            for (int i = 0; i < distance.Count; i++)
            {
                if (clutterClasses[i] == clutter.Id)
                {
                    classH[i] = clutterHeights[i];
                }
            }

            if (!SKColor.TryParse((string)clutter.Color, out var sk))
                sk = SKColors.Gray;

            series.Add(new LineSeries<ObservablePoint>
            {
                Name = clutter.Name,
                Values = PointsAllowNull(distance, classH),
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(sk.WithAlpha(128)),
                Fill = new SolidColorPaint(sk.WithAlpha(128)),
            });
        }

        return series;
    }

    private static ObservablePoint[] Points(List<double> xs, List<double> ys)
    {
        var arr = new ObservablePoint[xs.Count];
        for (int i = 0; i < xs.Count; i++) arr[i] = new(xs[i], ys[i]);
        return arr;
    }

    private static ObservablePoint[] PointsAllowNull(List<double> xs, List<double?> ys)
    {
        var arr = new ObservablePoint[xs.Count];
        for (int i = 0; i < xs.Count; i++) arr[i] = new(xs[i], ys[i]);
        return arr;
    }
}
