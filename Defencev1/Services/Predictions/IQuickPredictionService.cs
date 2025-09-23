using Defencev1.Models;
using Defencev1.Utils.Result;

namespace Defencev1.Services.Predictions;

public interface IQuickPredictionService
{
    Task<Result<string>> PostQuickPrediction(long workspaceId, QuickRfRequest quickRfModel);
    Task<Result<string>> GetPredictionRaster(long workspaceId, long taskId, string fileName, bool volatility = true);
}
