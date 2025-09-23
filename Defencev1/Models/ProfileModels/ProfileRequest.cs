using Newtonsoft.Json;

namespace Defencev1.Models.ProfileModels;

public class ProfileRequest
{


    [JsonProperty("transmitter")]
    public required Transmitter Transmitter { get; set; }

    [JsonProperty("receiver")]
    public required Receiver Receiver { get; set; }
}

