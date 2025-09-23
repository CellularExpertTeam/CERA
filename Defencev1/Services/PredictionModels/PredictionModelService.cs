using Defencev1.Models;
using Defencev1.Services.Auth;
using Defencev1.Utils.Result;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Defencev1.Services.PredictionModels;

public class PredictionModelService(
    HttpClient httpClient,
    IAuthService authService, 
    ILogger<PredictionModelService> logger) : IPredictionModelService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IAuthService _authService = authService;
    private readonly ILogger<PredictionModelService> _logger = logger;

    /// <summary>
    /// Gets all the prediction models and their configurations from CE Express API.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a  <see
    /// cref="Result{T}"/> object that holds the <see cref="PredictionModelsResponse"/> if the operation succeeds,  or an error
    /// message if it fails.</returns>
    public async Task<Result<PredictionModelsResponse>> GetPredictionModels()
    {
        try
        {
            var token = await SecureStorage.GetAsync("CE_TOKEN");
            if (token == null)
            {
                _logger.LogError("Trying to call GetPredictionModels() while not logged in.");
            }
            string url = $"{_authService.CEURL}/predictionModels?token={token}";
            HttpResponseMessage? response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PredictionModelsResponse>(json);
                if (obj is null)
                {
                    _logger.LogError("Failed to deserialize prediction model response");
                    return Result<PredictionModelsResponse>.Fail("Failed to read prediction models.");
                }
                return Result<PredictionModelsResponse>.Ok(obj);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            return Result<PredictionModelsResponse>.Fail($"Failed to retrieve prediction models, reason: {errorMessage}");
        }
        catch (Exception ex)
        {
            return Result<PredictionModelsResponse>.Fail($"Error while retrieving prediction models: {ex.Message}");
        }
    }
}
