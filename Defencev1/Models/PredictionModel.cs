using Newtonsoft.Json;

namespace Defencev1.Models;

public class PredictionModel
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("configurations")]
    public List<PredictionModelConfig> Configurations { get; set; }

    public override string ToString()
    {
        return Name;
    }
}


public class PredictionModelConfig
{
    [JsonProperty("object_id")]
    public string Id { get; set; }

    [JsonProperty("configurationName")]
    public string Name { get; set; }

    [JsonProperty("max_radius")]
    public double MaxRadius { get; set; }

    [JsonProperty("receiver_height")]
    public double ReceiverHeight { get; set; }

    [JsonProperty("effective_earth_radius")]
    public double EffectiveEearthRadius { get; set; }

    [JsonProperty("Clutter_values")]
    public string ClutterValues { get; set; }

    [JsonProperty("offset_coeficient")]
    public double OffsetCoefficient { get; set; }

    [JsonProperty("slope_coefficient_distance")]
    public double SlopeCoefDist { get; set; }

    [JsonProperty("slope_coefficient_frequency")]
    public double SlopeCoefFreq { get; set; }

    [JsonProperty("time_percentage")]
    public double TimePercentage { get; set; }

    [JsonProperty("use_multipath_focusing")]
    public bool UseMultiPathFocusing { get; set; }
}

public class PredictionModelsResponse
{
    [JsonProperty("data")]
    public Dictionary<string, PredictionModel> Data { get; set; }
}