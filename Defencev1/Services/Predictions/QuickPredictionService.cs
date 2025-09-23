using Defencev1.Models;
using Defencev1.Services.Auth;
using Defencev1.Services.Workspaces;
using Defencev1.Utils.Result;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Defencev1.Services.Predictions;

public class QuickPredictionService(
    HttpClient httpClient, 
    IAuthService authService,
    IWorkspaceService workspaceService, 
    ILogger<QuickPredictionService> logger) : IQuickPredictionService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IAuthService _authService = authService;
    private readonly IWorkspaceService _workspaceService = workspaceService;
    private readonly ILogger<QuickPredictionService> _logger = logger;

    /// <summary>
    /// Posts a request to CE Express API to calculate quick rf prediction result. The resulting raster get's saved
    /// in CE Express and can be accessed using <see cref="GetPredictionRaster"/> method. 
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace to which the profile request is sent.</param>
    /// <param name="quickRfRequest">Quick rf prediction request model, containing the cell, cell size and calculation name.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that contains the raw json response is the operation succeeds, or an error message
    /// if it fails.</returns>
    public async Task<Result<string>> PostQuickPrediction(long workspaceId, QuickRfRequest quickRfRequest)
    {
        if (workspaceId <= 0)
            return Result<string>.Fail("Workspace ID must be greater than zero.");
        if (quickRfRequest is null)
            return Result<string>.Fail("QuickRfRequest model cannot be null.");


        string? token = await SecureStorage.GetAsync("CE_TOKEN");

        var response = await _httpClient.PostAsJsonAsync(
        $"{_authService.CEURL}/{workspaceId}/calculations/quickRf?token={token}",
        quickRfRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(result);
            return Result<string>.Ok(result);
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Error posting quick prediction: {errorMessage}");
            return Result<string>.Fail(errorMessage);
        }
    }

    /// <summary>
    /// Gets a task result from a previously made calculation.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace to which the profile request is sent.</param>
    /// <param name="quickRfRequest">Quick rf prediction request model, containing the cell, cell size and calculation name.</param>
    /// <param name="fileName">File name of the prediction result raster file (for quick prediction always cell_0_fs1.tif).</param>
    /// <param name="volatility">For tasks that appear in prediction history should be set to false, othervise true</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="Result{T}"/> object that contains the raster file path if operation succeeds, or an error message
    /// if it fails.</returns>
    public async Task<Result<string>> GetPredictionRaster(long workspaceId, long taskId, string fileName, bool volatility = true)
    {
        if (workspaceId <= 0 || string.IsNullOrEmpty(_workspaceService.ActiveWorkspace.LocalDirectory))
            return Result<string>.Fail("Could not find active workspace");

        if (taskId <= 0)
            return Result<string>.Fail("Task ID must be greater than zero.");

        try
        {
            var token = await SecureStorage.GetAsync("CE_TOKEN");
            var response = await _httpClient.GetAsync(
           $"{_authService.CEURL}/{workspaceId}/calculations/tasks/{taskId}/results/tif/{fileName}?token={token}&volatile={volatility}");

            if (response.IsSuccessStatusCode)
            {
                string taskFileName = $"task_{taskId}_{fileName}";
                using var stream = await response.Content.ReadAsStreamAsync();
                var filePath = Path.Combine(_workspaceService.ActiveWorkspace.LocalDirectory, taskFileName);
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);
                return Result<string>.Ok(filePath);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting quickRf result: {errorMessage}");
                return Result<string>.Fail(errorMessage);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<string>.Fail(e.Message);
        }
    }
}