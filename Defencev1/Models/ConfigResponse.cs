using Newtonsoft.Json;

namespace Defencev1.Models;

public class ConfigResponse
{
    [JsonProperty("portalUrl")]
    public string PortalUrl { get; set; }

    [JsonProperty("ceApiUrl")]
    public string CeApiUrl { get; set; }

    [JsonProperty("inventoryOrigin")]
    public string InventoryOrigin { get; set; }

    [JsonProperty("googleMapsApiKey")]
    public string GoogleMapsApiKey { get; set; }
}
