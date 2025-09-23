using Defencev1.Models;
using Defencev1.Utils.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Defencev1.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    public string CEURL { get; set; }
    public string PortalURL { get; set; }
    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Authenticates the user using the provided credentials and retrieves a token for accessing the API.
    /// </summary>
    /// <remarks>This method performs a two-step authentication process: 1. It generates a token from the
    /// portal using the provided credentials. 2. It uses the generated token to log in to the API and retrieve a
    /// temporary token.  The temporary token is securely stored for future use. If the service is unavailable or an
    /// error occurs  during the process, the method logs the error and returns a failure result.</remarks>
    /// <param name="credentials">The user's credentials, including the portal URL, username, and password.</param>
    /// <returns>A <see cref="Result{T}"/> containing a success message if the authentication is successful,  or an error message
    /// if the operation fails.</returns>
    public async Task<Result<string>> GenerateCEToken(Credentials credentials)
    {
        try
        {
            var tokenResult = await GeneratePortalToken(credentials.PortalUrl, credentials.UserName, credentials.Password);
            if (!tokenResult.IsSuccess)
                return Result<string>.Fail(tokenResult.Error);

            string url = $"{credentials.ApiUrl}/users/login";
            var password = await SecureStorage.GetAsync("PORTAL_TOKEN");
            LoginRequest loginRequest = new() { Email = credentials.UserName, Password = password};
             
            // Login using Portal OAuth (use portal's returned token as a password)
            var response = await _httpClient.PostAsJsonAsync(url, loginRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError(response.ReasonPhrase);
                return Result<string>.Fail("Cellular-expert service unavailable.");
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(json);
            LoginResponse loginResponse = JsonSerializer.Deserialize<LoginResponse>(json);

            var tempToken = loginResponse?.Data?.Token;
            if (string.IsNullOrWhiteSpace(tempToken))
            {
                return Result<string>.Fail($"Failed to log in: {json}");
            }
            else
            {
                await SecureStorage.SetAsync("CE_TOKEN", tempToken);
            }
            CEURL = credentials.ApiUrl;
            PortalURL = credentials.PortalUrl;
            return Result<string>.Ok("Logged in");
        }
        catch (Exception e)
        {
            _logger.LogError($"{e.Message}, url: {credentials.ApiUrl}, username: {credentials.UserName}");
            return Result<string>.Fail(e.Message);
        }
    }

    /// <summary>
    /// Terminate auth tokens
    /// </summary>
    public void LogOut()
    {
        SecureStorage.Remove("CE_TOKEN");
        SecureStorage.Remove("PORTAL_TOKEN");
    }

    /// <summary>
    /// Generates ArcGIS portal token. This token is used as a password to login to CE Express.
    /// </summary>
    /// <param name="portalUrl"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<Result<string>> GeneratePortalToken(string portalUrl, string username, string password)
    {
        string url = $"{portalUrl}/sharing/rest/generateToken";

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("request", "getToken"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("expiration", "300"),
            new KeyValuePair<string, string>("referer", "CE API"),
            new KeyValuePair<string, string>("f", "json"),
        });
        
        var response = await _httpClient.PostAsync(url, formData);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

        if (result == null || !result.ContainsKey("token") || string.IsNullOrEmpty(result["token"].ToString()))
        {
            _logger.LogError(jsonString);
            return Result<string>.Fail(jsonString);
        }

        await SecureStorage.SetAsync("PORTAL_TOKEN", result["token"].ToString());
        return Result<string>.Ok("Portal token generated");
    }

    public async Task<Result<ConfigResponse>> GetConfig(string url)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ConfigResponse>(url);

            if (!string.IsNullOrEmpty(response.CeApiUrl))
            {
                return Result<ConfigResponse>.Ok(response);
            }
            else
            {
                return Result<ConfigResponse>.Fail("Failed to retrieve CE config.");
            }
        }
        catch(Exception e)
        {
            return Result<ConfigResponse>.Fail(e.Message);
        }
        
    }
}
