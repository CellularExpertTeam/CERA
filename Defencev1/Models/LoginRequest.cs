using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Defencev1.Models;

public class LoginRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "esri";
    [JsonPropertyName("password")]
    public string Password { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
}
