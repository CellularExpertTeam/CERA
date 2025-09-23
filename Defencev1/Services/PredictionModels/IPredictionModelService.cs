using Defencev1.Models;
using Defencev1.Utils.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Services.PredictionModels;

public interface IPredictionModelService
{
    Task<Result<PredictionModelsResponse>> GetPredictionModels();
}
