using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Defencev1.Models.ProfileModels;

public partial class Receiver : ObservableObject
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
    [property: JsonProperty("antenna_id")]
    private string antennaId = "16";

    [ObservableProperty]
    [property: JsonProperty("power")]
    private int power = 20;

    [ObservableProperty]
    [property: JsonProperty("misc_loss")]
    private int miscLoss;
}
