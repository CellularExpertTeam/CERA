using CommunityToolkit.Maui;
using Defencev1.Controllers.MapController;
using Defencev1.Services.Auth;
using Defencev1.Services.Cells;
using Defencev1.Services.PredictionModels;
using Defencev1.Services.Predictions;
using Defencev1.Services.Profile;
using Defencev1.Services.Workspaces;
using Defencev1.ViewModels;
using Defencev1.ViewModels.Predictions;
using Defencev1.Views;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Toolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;

namespace Defencev1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Debug.WriteLine($"[Domain.Unhandled] {ex}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Debug.WriteLine($"[Task.Unobserved] {e.Exception}");
            // Prevents the process from being torn down by GC surfacing this later
            e.SetObserved();
        };

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseArcGISToolkit()
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        string logPath = Path.Combine(FileSystem.AppDataDirectory, "logs.log");
// Save debug logs in current project directory
#if DEBUG
        var buildDir = AppContext.BaseDirectory;
        string binDebugDirectory = Path.GetDirectoryName(buildDir)!;
        string projectRoot = Path.GetFullPath(Path.Combine(binDebugDirectory, "..", "..", "..", ".."));
        logPath = Path.Combine(projectRoot, "DebugLogs", "debug_log.log");
#endif

        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
#if DEBUG
                .WriteTo.Debug()
#endif
                .CreateLogger();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        builder.Configuration.AddConfiguration(config);

        builder.Services.AddTransient<LoggingHandler>();
        builder.Logging.AddSerilog(dispose: true);

        /// 
        /// Api key required to use Online mode. For more information visit: 
        ///
        string esriApiKey = builder.Configuration["EsriApiKey"];
        ///
        ///
        ///

        builder
        .UseArcGISRuntime(config => config
            .UseApiKey(esriApiKey)
            .ConfigureAuthentication(auth => auth
                .UseDefaultChallengeHandler()
            )
        )
        .SetProjectionEnginePath()
        .RegisterServices()
        .RegisterViews()
        .RegisterViewModels()
        .Services.AddSingleton<HttpClient>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingHandler>>();
            var handler = new LoggingHandler(new HttpClientHandler(), logger);
            return new HttpClient(handler);
        });
            
        try
        {
            ArcGISRuntimeEnvironment.EnableTimestampOffsetSupport = true;
            //ArcGISRuntimeEnvironment.SetLicense(builder.Configuration["EsriRuntimeLicense"]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArcGIS Init] {ex}");
        }
        return builder.Build();
    }

    public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ICellService, CellService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IWorkspaceService, WorkspaceService>();
        builder.Services.AddSingleton<IProfileService, ProfileService>();
        builder.Services.AddSingleton<IMapController, MapController>();
        builder.Services.AddSingleton<IPredictionModelService, PredictionModelService>();

        builder.Services.AddTransient<IQuickPredictionService, QuickPredictionService>();
        builder.Services.AddTransient<LoggingHandler>();

        return builder;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<ProfilePaneView>();
        builder.Services.AddSingleton<SettingsPaneView>();
        builder.Services.AddSingleton<WorkspacePaneView>();
        builder.Services.AddSingleton<LayerPaneView>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<UserProfileView>();
        builder.Services.AddSingleton<FilesPaneView>();

        builder.Services.AddTransient<QuickPredictionView>();
        builder.Services.AddTransient<ProfileGraphView>();
        return builder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<ProfilePaneViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<WorkspaceViewModel>();
        builder.Services.AddSingleton<LayerViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<UserProfileViewModel>();
        builder.Services.AddSingleton<FilesPaneViewModel>();

        builder.Services.AddTransient<ProfileGraphViewModel>();
        builder.Services.AddTransient<QuickPredictionViewModel>();
        return builder;
    }

    // If you are working only with Web Mercator (3857) and WGS84 (4326) spatial references
    // setting up Projection Engine data is not necessary
    public static MauiAppBuilder SetProjectionEnginePath(this MauiAppBuilder builder)
    {
        string projectionEngineDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "Projection_Engine_Data_200_8_0", "pedata");
        if (!Directory.Exists(projectionEngineDataPath))
        {
            Debug.WriteLine("Projection engine data missing. Errors with some Spatial references may occur.");
            return builder;
        }

        TransformationCatalog.ProjectionEngineDirectory = projectionEngineDataPath;
        return builder;
    }
}
