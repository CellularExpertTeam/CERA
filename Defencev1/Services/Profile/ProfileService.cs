using Defencev1.Models.ProfileModels;
using Defencev1.Services.Auth;
using Defencev1.Utils.Result;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Defencev1.Services.Profile;

public class ProfileService 
    (HttpClient httpClient, IAuthService authService) : IProfileService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IAuthService _authService = authService;
    private ProfileResponse? _currentProfile;

    public ProfileResponse? CurrentProfile
    {
        get { return _currentProfile; }
        set { _currentProfile = value; }
    }

    /// <summary>
    /// Sends a profile request to the specified workspace and retrieves the resulting profile data.
    /// </summary>
    /// <remarks>This method sends an HTTP POST request to the specified workspace endpoint, including the
    /// profile request  data serialized as JSON. The method requires a valid token. If the request is successful, the resulting profile data is stored in the
    /// <see cref="CurrentProfile"/> property.</remarks>
    /// <param name="workspaceId">The unique identifier of the workspace to which the profile request is sent.</param>
    /// <param name="profileRequest">The profile request data to be sent in the HTTP POST request.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a  <see
    /// cref="Result{T}"/> object that holds the <see cref="ProfileResponse"/> if the operation succeeds,  or an error
    /// message if it fails.</returns>
    public async Task<Result<ProfileResponse>> PostProfile(long workspaceId, ProfileRequest profileRequest)
    {
        try
        {
            var json = JsonConvert.SerializeObject(profileRequest);
            HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var token = await SecureStorage.GetAsync("CE_TOKEN");
            var response = await _httpClient.PostAsync(
           $"{_authService.CEURL}/{workspaceId}/calculations/profile?token={token}",
           content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ProfileResponse>();
                CurrentProfile = result;
                return Result<ProfileResponse>.Ok(result);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return Result<ProfileResponse>.Fail(errorMessage);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return Result<ProfileResponse>.Fail($"Exception occurred: {e.Message}");
        }
    }

    public static Result<string> Validate(ProfileRequest request)
    {
        if (request.Transmitter.X == 0 || request.Transmitter.Y == 0)
            return Result<string>.Fail("Invalid transmitter coordinates");

        if (request.Receiver.X == 0 || request.Receiver.Y == 0)
            return Result<string>.Fail("Invalid transmitter coordinates");

        return Result<string>.Ok("Profile valid.");
    }
}
