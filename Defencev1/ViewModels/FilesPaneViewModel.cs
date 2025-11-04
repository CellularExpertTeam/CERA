using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Controllers.MapController;
using Defencev1.Messages;
using Defencev1.Services.Workspaces;
using Defencev1.Utils.Result;
using Defencev1.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Defencev1.ViewModels;

public partial class FilesPaneViewModel : ViewModelBase
{
    private readonly ILogger<FilesPaneViewModel> _logger;
    private readonly IWorkspaceService _workspaceService;
    public IMapController MapController { get; }
    public FilesPaneViewModel(
        IMapController mapController,
        IWorkspaceService workspaceService,
        ILogger<FilesPaneViewModel> logger)
    {
        MapController = mapController;
        _workspaceService = workspaceService;
        _logger = logger;

        WeakReferenceMessenger.Default.Register<NewWorkspaceOpenedMsg>(this, (_, __) => EnableMmpkLoading());
    }

    [ObservableProperty]
    private ObservableCollection<MmpkFile> _loadedMapPackageFiles = [];

    [ObservableProperty]
    private string _selectedMapPackagePath;

    [ObservableProperty]
    private string _resultMsg = string.Empty;

    [RelayCommand]
    private async Task SelectFiles()
    {
        PickOptions options = new();

        try
        {
            var result = await FilePicker.PickMultipleAsync(options);
            if (result != null)
            {
                foreach (var file in result)
                {
                    if (file.FileName.EndsWith("mmpk", StringComparison.OrdinalIgnoreCase))
                    {
                        MmpkFile temp = new();
                        temp.FullPath = file.FullPath;
                        temp.Name = file.FileName;

                        if (_workspaceService.ActiveWorkspace is null)
                        {
                            temp.IsLoadingEnabled = false;
                            ResultMsg = "Please open a workspace to enable loading map packages.";
                        }

                        LoadedMapPackageFiles.Add(temp);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {e.Message}", e.Message);
        }
    }

    private void EnableMmpkLoading()
    {
        foreach (var item in LoadedMapPackageFiles)
            item.IsLoadingEnabled = true;

        ResultMsg = string.Empty;
    }


    public async Task LoadMmpk(string fullPath)
    {
        try
        {
            Result<string> result = await MapController.LoadMmpkMap(fullPath, _workspaceService.ActiveWorkspace);
            if (result.IsSuccess)
            {
                ResultMsg = result.Value;
                return;
            }
            ResultMsg = result.Error;
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {e.Message}", e.Message);
            ResultMsg = e.Message;
        }
    }
}

public partial class MmpkFile : ObservableObject
{
    public string Name { get; set; }
    public string FullPath { get; set; }

    [ObservableProperty]
    private bool _isLoadingEnabled = true;
}
