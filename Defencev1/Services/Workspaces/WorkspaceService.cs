using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Models;
using Defencev1.Services.Auth;
using Defencev1.Utils;
using Defencev1.Utils.Result;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Defencev1.Services.Workspaces;

public partial class WorkspaceService : IWorkspaceService
{
    private readonly ILogger<WorkspaceService> _logger;
    private readonly IAuthService _authService;
    private Workspace? _activeWorkspace;
    private HttpClient _httpClient;

    public WorkspaceService(
        ILogger<WorkspaceService> logger,
        HttpClient httpClient,
        IAuthService authService) 
    {
        _logger = logger;
         _authService = authService;
	     _httpClient= httpClient;
    }

    public Workspace? ActiveWorkspace
	{
		get { return _activeWorkspace; }
		set { _activeWorkspace = value; }
	}

    /// <summary>
    /// Gets a worskpace object corresponding to given Id.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that contains success message if operation succeeds, or an error message
    /// if it fails.</returns>
    public async Task<Result<string>> GetWorkspaceById(long id)
    {
        if (id < 1)
            return Result<string>.Fail("Invalid workspace id");

        try
        {
            var token = await SecureStorage.GetAsync("CE_TOKEN");
            string url = $"{_authService.CEURL}/workspaces/{id}?token={token}";
            HttpResponseMessage? response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<WorkspaceResponse>(json);
                if (obj is null)
                {
                    _logger.LogError("Failed to deserialize workspace response");
                    return Result<string>.Fail("Failed to read workspace");
                }
                ActiveWorkspace = obj.WorkspaceData;
                ActiveWorkspace.GetOrCreateWorkspaceDir();

                return Result<string>.Ok("Successfully retrieved workspace");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            return Result<string>.Fail($"Failed to retrieve workspace, reason: {errorMessage}");
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {e.Message}", e.Message);
            return Result<string>.Fail($"Error while retrieving workspace");
        }
    }

    /// <summary>
    /// Gets all workspaces saved in CE Express.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that contains a list of workspace objects if operation succeeds, or an error message
    /// if it fails.</returns>
    public async Task<Result<List<Workspace>>> GetWorkspaces()
    {
        try
        {
            var token = await SecureStorage.GetAsync("CE_TOKEN");
            string url = $"{_authService.CEURL}/workspaces?token={token}";
            HttpResponseMessage? response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<WorkspacesResponse>(json);
                if (obj is null)
                {
                    _logger.LogError("Failed to deserialize workspaces response");
                    return Result<List<Workspace>>.Fail("Failed to read workspaces");
                }
                return Result<List<Workspace>>.Ok(obj.Workspaces);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            return Result<List<Workspace>>.Fail($"Failed to retrieve workspace, reason: {errorMessage}");
        }
        catch (Exception ex)
        {
            return Result<List<Workspace>>.Fail($"Error while retrieving workspace: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets all geodatasets assigned to a workspace and saves them in AppDataDirectory, Geodatasets folder.
    /// If the geodatasets are already saved, the download is skipped.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that contains a list of geodataset raster paths if operation succeeds, or an error message
    /// if it fails.</returns>
    public async Task<Result<List<string>>> GetGeodatasets()
    {
        if (ActiveWorkspace is null)
            return Result<List<string>>.Fail("No workspace selected");

        if (ActiveWorkspace.GeodatasetId == 0)
            return Result<List<string>>.Fail("Workspace does not have any geodatasets");

        try
        {
            string[] fileNames = ["elevation.tif", "clutterHeight.tif", "clutterClasses.tif"];
            string url = string.Empty;
            List<string> files = [];

            foreach (string fileName in fileNames)
            {
                // Check if we already have the geodatasets saved locally
                var filePath = Path.Combine(FileSystemUtils.GetOrCreateGeodatasetDirectory(ActiveWorkspace.GeodatasetId), fileName);
                if (File.Exists(filePath))
                {
                    files.Add(filePath);
                    continue;
                }

                var token = await SecureStorage.GetAsync("CE_TOKEN");
                // Download the geodatasets
                url = $"{_authService.CEURL}/geodataSets/{ActiveWorkspace.GeodatasetId}/tif/{fileName}?token={token}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = File.Create(filePath);
                    await stream.CopyToAsync(fileStream);
                    files.Add(filePath);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error getting quickRf result: {errorMessage}");
                }
            }

            return Result<List<string>>.Ok(files);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<string>>.Fail($"Failed to retrieve geodatasets. {e.Message}");
        }
    }

    /// <summary>
    /// Creates a ArcGIS Layer object, from a given url or a resource object id.
    /// </summary>
    /// <param name="layer" layer uri or id>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a
    /// layer object if operation succeeds, or an error message if it fails.</returns>
    public async Task<Layer?> CreateLayerFromUrl(string layer)
    {
        try
        {
            // Get features from FeatureServer
            if (Uri.IsWellFormedUriString(layer, UriKind.Absolute))
            {
                var uri = new Uri(layer, UriKind.Absolute);

                if (layer.Contains("FeatureServer", StringComparison.OrdinalIgnoreCase))
                {
                    return new FeatureLayer(new ServiceFeatureTable(uri));
                }
            }
            //Get features from portal using id, requires to log into portal
            else
            {
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(_authService.PortalURL));
                PortalItem item = await PortalItem.CreateAsync(portal, layer);
                return new FeatureLayer(item);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating layer from URL {layer}: {ex.Message}");
            return null;
        }
    }
}
