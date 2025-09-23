using Defencev1.Models;
using Defencev1.Services.Auth;
using Defencev1.Utils.Result;
using System.Net.Http.Json;

namespace Defencev1.Services.Cells;

public class CellService(HttpClient httpClient, IAuthService authService) : ICellService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Adds a new cell to the specified workspace.
    /// </summary>
    /// <remarks>This method sends an HTTP POST request to add the specified cell(s) to the workspace. The
    /// request requires a valid token, which is retrieved from secure storage.</remarks>
    /// <param name="workspaceId">The unique identifier of the workspace. Must be greater than zero.</param>
    /// <param name="request">The request containing the details of the cell(s) to be added. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Result{T}"/> containing the result of the operation. If successful, the result contains the
    /// response string from the server. Otherwise, it contains an error message describing the failure.</returns>
    public async Task<Result<string>> AddNewCellToWorkspace(long workspaceId, CellAddRequest request)
    {
        if (request is null)
            return Result<string>.Fail("Cell cannot be null.");

        if (workspaceId <= 0)
            return Result<string>.Fail("Workspace ID must be greater than zero.");

        if (request.newCells.FirstOrDefault().X == 0 || request.newCells.FirstOrDefault().Y == 0)
            return Result<string>.Fail("Cell coordinates cannot be zero.");

        string token = await SecureStorage.GetAsync("CE_TOKEN");

       var response = await _httpClient.PostAsJsonAsync($"{_authService.CEURL}/{workspaceId}/features/cells?token={token}", request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return Result<string>.Fail($"Failed to add cell: {errorMessage}");
        }

        var result = await response.Content.ReadAsStringAsync();

        return Result<string>.Ok(result);
    }
}
