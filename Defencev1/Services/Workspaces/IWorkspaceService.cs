using Defencev1.Models;
using Defencev1.Utils.Result;
using Esri.ArcGISRuntime.Mapping;

namespace Defencev1.Services.Workspaces;

public interface IWorkspaceService
{
    Workspace? ActiveWorkspace { get; set; }
    Task<Result<string>> GetWorkspaceById(long id);
    Task<Result<List<Workspace>>> GetWorkspaces();
    Task<Layer?> CreateLayerFromUrl(string url);

    Task<Result<List<string>>> GetGeodatasets();
}
