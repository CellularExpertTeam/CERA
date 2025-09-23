using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Defencev1.Models;

public class QuickRfRequest
{
    [JsonPropertyName("calculationName")]
    public string CalculationName { get; } = "Quick prediction";

    [JsonPropertyName("cellSize")]
    public int CellSize { get; set; }

    [JsonPropertyName("cell")]
    public CE_Cell Cell { get; set; }
    
}

public class CellParameters
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; } = 30;

    [JsonPropertyName("azimuth")]
    public double Azimuth { get; set; } = 0;

    [JsonPropertyName("tilt")]
    public double Tilt { get; set; } = 0;

    [JsonPropertyName("electrical_tilt")]
    public double ElectricalTilt { get; set; } = 0;

    [JsonPropertyName("antenna_id")]
    public string AntennaId { get; set; } = "51";

    [JsonPropertyName("frequency")]
    public int Frequency { get; set; } = 2400;

    [JsonPropertyName("bandwidth")]
    public int Bandwidth { get; set; } = 20;

    [JsonPropertyName("power")]
    public int Power { get; set; } = 20;

    [JsonPropertyName("misc_loss")]
    public int MiscLoss { get; set; } = 0;

    [JsonPropertyName("tx_mimo")]
    public int TxMimo { get; set; } = 1;

    [JsonPropertyName("subcarrier_spacing")]
    public string SubcarrierSpacing { get; set; } = "15";

    [JsonPropertyName("prediction_model_id")]
    public int PredictionModelId { get; set; } = 3;

    [JsonPropertyName("prediction_model_configuration_id")]
    public string PredictionModelConfigurationId { get; set; } = "9";

    [JsonPropertyName("cell_name")]
    public string CellName { get => $"{X:F6}, {Y:F6}"; }
}
