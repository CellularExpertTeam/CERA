using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Defencev1.Models;

public class LoginResponse
{
    [JsonPropertyName("data")]
    public TokenData Data { get; set; }
}

public class TokenData
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
}