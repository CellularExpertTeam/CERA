
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Defencev1.Models;

public class CellAddRequest
{
    [JsonPropertyName("features")]
    public List<CE_Cell> newCells { get; set; }
}

public class NewCell
{
    [JsonPropertyName("cell_name")]
    public string CellName { get; set; }
    [JsonPropertyName("x")]
    public double X { get; set; }
    [JsonPropertyName("y")]
    public double Y { get; set; }
    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("azimuth")]
    public double Azimuth { get; set; }
}


