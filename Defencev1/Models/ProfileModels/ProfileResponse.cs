using System.Text.Json.Serialization;
using Defencev1.Models.ClutterModels;

namespace Defencev1.Models.ProfileModels;

public class ProfileResponse
{
    [JsonPropertyName("data")]
    public ProfileResponseBody Data { get; set; }
}

public class ProfileResponseBody
{
    [JsonPropertyName("arrays")]
    public Arrays Arrays { get; set; }

    [JsonPropertyName("traceValues")]
    public TraceValues TraceValues { get; set; }

    [JsonPropertyName("pathType")]
    public string PathType { get; set; }

    [JsonPropertyName("clutterClasses")]
    public Dictionary<string, Clutter> ClutterClasses { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("firstBObstIndex")]
    public int FirstBObstIndex { get; set; }

    [JsonPropertyName("firstCObstIndex")]
    public int FirstCObstIndex { get; set; }

    [JsonPropertyName("firstBObstDistance")]
    public double FirstBObstDistance { get; set; }

    [JsonPropertyName("firstCObstDistance")]
    public double FirstCObstDistance { get; set; }

    [JsonPropertyName("transmitterAzimuth")]
    public double TransmitterAzimuth { get; set; }

    [JsonPropertyName("transmitterTilt")]
    public double TransmitterTilt { get; set; }

    [JsonPropertyName("receiverAzimuth")]
    public double ReceiverAzimuth { get; set; }

    [JsonPropertyName("receiverTilt")]
    public double ReceiverTilt { get; set; }

    [JsonPropertyName("txToRxAzimuth")]
    public double TxToRxAzimuth { get; set; }

    [JsonPropertyName("txToRxTilt")]
    public double TxToRxTilt { get; set; }

    [JsonPropertyName("rxToTxAzimuth")]
    public double RxToTxAzimuth { get; set; }

    [JsonPropertyName("rxToTxTilt")]
    public double RxToTxTilt { get; set; }

    [JsonPropertyName("visibilityClearance")]
    public double VisibilityClearance { get; set; }

    [JsonPropertyName("clearance")]
    public double Clearance { get; set; }

    [JsonPropertyName("clearancePercentage")]
    public double ClearancePercentage { get; set; }

    [JsonPropertyName("clearanceDistance")]
    public double ClearanceDistance { get; set; }

    [JsonPropertyName("pathLossValues")]
    public PathLossValues PathLossValues { get; set; }

    [JsonPropertyName("downlinkFieldStrength")]
    public double DownlinkFieldStrength { get; set; }

    [JsonPropertyName("uplinkFieldStrength")]
    public double UplinkFieldStrength { get; set; }

    [JsonPropertyName("downlinkFwaRsl")]
    public double DownlinkFwaRsl { get; set; }

    [JsonPropertyName("uplinkFwaRsl")]
    public double UplinkFwaRsl { get; set; }
}


public class Arrays
{
    [JsonPropertyName("distance")]
    public List<double> Distance { get; set; }

    [JsonPropertyName("elevation")]
    public List<double> Elevation { get; set; }

    [JsonPropertyName("clutterHeight")]
    public List<double> ClutterHeight { get; set; }

    [JsonPropertyName("clutterClass")]
    public List<int> ClutterClass { get; set; }

    [JsonPropertyName("profile")]
    public List<double> Profile { get; set; }

    [JsonPropertyName("fresnel")]
    public List<double> Fresnel { get; set; }
}

public class TraceValues
{
    [JsonPropertyName("bObstH")]
    public double BObstH { get; set; }

    [JsonPropertyName("cObstH")]
    public double CObstH { get; set; }

    [JsonPropertyName("penObstH")]
    public double PenObstH { get; set; }

    [JsonPropertyName("bObstIndex")]
    public int BObstIndex { get; set; }

    [JsonPropertyName("cObstIndex")]
    public int CObstIndex { get; set; }

    [JsonPropertyName("penObstDepth")]
    public double PenObstDepth { get; set; }
}

public class PathLossValues
{
    [JsonPropertyName("pathLoss")]
    public double PathLoss { get; set; }

    [JsonPropertyName("basicLoss")]
    public double BasicLoss { get; set; }

    [JsonPropertyName("diffractionLoss")]
    public double DiffractionLoss { get; set; }

    [JsonPropertyName("clutterLoss")]
    public double ClutterLoss { get; set; }

    [JsonPropertyName("penetrationLoss")]
    public double PenetrationLoss { get; set; }

    [JsonPropertyName("penObstDiffLoss")]
    public double PenObstDiffLoss { get; set; }

    [JsonPropertyName("receiverClutterLoss")]
    public double ReceiverClutterLoss { get; set; }
}
