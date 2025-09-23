using Defencev1.Utils.Result;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System.Diagnostics;
using Mapping = Esri.ArcGISRuntime.Mapping;

namespace Defencev1.Services.EsriMap;

public class MapService : IMapService
{
    public async Task<Result<Mapping.Map>> FetchMapByTitle(string portalUrl, string webMapTitle)
    {
        try
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(portalUrl));
            PortalQueryParameters query = new(
            $"(title:\"{webMapTitle}\") AND (type:\"Web Map\")")
            {
                Limit = 5,
            };

            var results = await portal.FindItemsAsync(query);

            var item = results.Results
                .FirstOrDefault(i => i.Type == PortalItemType.WebMap);

            if (item == null)
                return Result<Mapping.Map>.Fail("Web map not found.");

            var importedMap = new Mapping.Map(item);

            return Result<Mapping.Map>.Ok(importedMap);
        }
        catch (Exception ex)
        {
            return Result<Mapping.Map>.Fail($"Error loading web map: {ex.Message}");
        }
    }

    public async Task<Result<Mapping.Map>> FetchMapById(string portalUrl, string webMapId)
    {
        try
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(portalUrl));
            PortalItem portalItem = await PortalItem.CreateAsync(portal, webMapId);

            if (portalItem.Type == PortalItemType.WebMap)
            {
                Mapping.Map webMap = new Mapping.Map(portalItem);
                Debug.Write("Successfully loaded the Web Map.");

                return Result<Mapping.Map>.Ok(webMap);
            }
            else
            {
                return Result<Mapping.Map>.Fail($"The item with ID '{portalItem?.ItemId}' is not a Web Map. It is a '{portalItem?.Type}'.");
            }
        }
        catch (Exception ex)
        {
            return Result<Mapping.Map>.Fail($"Error loading web map: {ex.Message}");
        }
    }

    public async Task<Result<Mapping.Map>> LoadMmpk(string fileName)
    {
        string mmpkPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        var mobileMapPackage = new MobileMapPackage(mmpkPath);
        await mobileMapPackage.LoadAsync();

        if (mobileMapPackage?.Maps.Count > 0)
        {
            Mapping.Map map = mobileMapPackage.Maps[0];

            return Result<Mapping.Map>.Ok(map);
        }
        else
        {
            return Result<Mapping.Map>.Fail("No maps found in package.");
        }

    }

}
