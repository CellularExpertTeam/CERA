using System.Text.Json.Serialization;

namespace Defencev1.Models;

public class CE_Cell
{
    
    [JsonPropertyName("cell_name")]
    public string CellName { get => $"{X:F6}, {Y:F6}"; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; } = 1;

    [JsonPropertyName("azimuth")]
    public double Azimuth { get; set; } = 0;

    [JsonPropertyName("tilt")]
    public double Tilt { get; set; } = 0;

    [JsonPropertyName("electrical_tilt")]
    public double ElectricalTilt { get; set; } = 0;

    [JsonPropertyName("antenna_id")]
    public long AntennaId { get; set; } = 468;

    [JsonPropertyName("frequency")]
    public double Frequency { get; set; } = 2400;

    [JsonPropertyName("bandwidth")]
    public double Bandwidth { get; set; } = 5;

    [JsonPropertyName("power")]
    public double Power { get; set; } = 20;

    [JsonPropertyName("misc_loss")]
    public double MiscLoss { get; set; } = 0;

    [JsonPropertyName("tx_mimo")]
    public long TxMimo { get; set; } = 1;

    [JsonPropertyName("subcarrier_spacing")]
    public long SubcarrierSpacing { get; set; } = 15;

    [JsonPropertyName("prediction_model_id")]
    public long PredictionModelId { get; set; } = 3;

    [JsonPropertyName("prediction_model_configuration_id")]
    public long PredictionModelConfigurationId { get; set; } = 19;

}