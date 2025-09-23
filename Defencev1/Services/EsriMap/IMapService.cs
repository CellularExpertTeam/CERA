using Mapping = Esri.ArcGISRuntime.Mapping;
using Defencev1.Utils.Result;

namespace Defencev1.Services.EsriMap
{
    public interface IMapService
    {
        Task<Result<Mapping.Map>> FetchMapByTitle(string portalUrl, string webMapTitle);
        Task<Result<Mapping.Map>> FetchMapById(string portalUrl, string webMapId);
        Task<Result<Mapping.Map>> LoadMmpk(string fileName);
    }
}
