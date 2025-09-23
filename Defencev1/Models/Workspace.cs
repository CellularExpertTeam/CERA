using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Defencev1.Models;

public class Workspace
{
    [JsonProperty("object_id")]
    public long Id { get; set; }

    [JsonProperty("workspace_name")]
    public string Name { get; set; } = "Workspace";

    [JsonProperty("extent_xmin")]
    public double ExtentXmin { get; set; }

    [JsonProperty("extent_ymin")]
    public double ExtentYmin { get; set; }

    [JsonProperty("extent_xmax")]
    public double ExtentXmax { get; set; }

    [JsonProperty("extent_ymax")]
    public double ExtentYmax { get; set; }

    [JsonProperty("epsg")]
    public int Epsg { get; set; }

    [JsonProperty("extra_layers")]
    public string ExtraLayers { get; set; } = string.Empty;

    [JsonProperty("use_clutter")]
    public bool UseClutter { get; set; } = true;

    [JsonProperty("calculate_eirp")]
    public bool CalculateEirp { get; set; } = false;

    [JsonProperty("receiver_height_reference")]
    public string ReceiverHeightReference { get; set; } = "elevation";

    [JsonProperty("geodata_set_id")]
    public long GeodatasetId { get; set; }

    [JsonProperty("transmitter_height_reference")]
    public string TransmitterHeightReference { get; set; } = "elevation";

    [JsonIgnore]
    public string LocalDirectory { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"({Id}) {Name}";
    }

    public void GetOrCreateWorkspaceDir()
    {
        string workspacePath = Path.Combine(FileSystem.AppDataDirectory, $"Workspace{Id}");
        if (Directory.Exists(workspacePath))
        {
            LocalDirectory = workspacePath;
            return;
        }

        try
        {
            Directory.CreateDirectory(workspacePath);
            LocalDirectory = workspacePath;
        }
        catch (Exception e)
        {
            Debug.Write(e);
        }
    }
}

public class WorkspaceResponse
{
    [JsonProperty("data")]
    public required Workspace WorkspaceData { get; set; }
}

public class WorkspacesResponse
{
    [JsonProperty("data")]
    public required List<Workspace> Workspaces { get; set; }
}


