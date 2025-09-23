using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Defencev1.Models.ProfileModels;

public partial class Transmitter : ObservableObject
{
    [ObservableProperty]
    [property: JsonProperty("x")]
    private double x;

    [ObservableProperty]
    [property: JsonProperty("y")]
    private double y;

    [ObservableProperty]
    [property: JsonProperty("height")]
    private int height = 30;

    [ObservableProperty]
    [property: JsonProperty("azimuth")]
    private int azimuth;

    [ObservableProperty]
    [property: JsonProperty("tilt")]
    private int tilt;

    [ObservableProperty]
    [property: JsonProperty("electrical_tilt")]
    private int electricalTilt;

    [ObservableProperty]
    [property: JsonProperty("antenna_id")]
    private string antennaId = "51";

    [ObservableProperty]
    [property: JsonProperty("frequency")]
    private int frequency = 800;

    [ObservableProperty]
    [property: JsonProperty("bandwidth")]
    private int bandwidth = 20;

    [ObservableProperty]
    [property: JsonProperty("power")]
    private int power = 43;

    [ObservableProperty]
    [property: JsonProperty("misc_loss")]
    private int miscLoss;

    [ObservableProperty]
    [property: JsonProperty("tx_mimo")]
    private int txMimo = 1;

    [ObservableProperty]
    [property: JsonProperty("rx_mimo")]
    private int rxMimo = 1;

    [ObservableProperty]
    [property: JsonProperty("subcarrier_spacing")]
    private string subcarrierSpacing = "15";

    [ObservableProperty]
    [property: JsonProperty("prediction_model_id")]
    private int predictionModelId = 3;

    [ObservableProperty]
    [property: JsonProperty("prediction_model_configuration_id")]
    private string predictionModelConfigurationId = "2";
}
