using Defencev1.Models;
using Defencev1.Utils.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Services.Auth;

public interface IAuthService
{
    Task<Result<string>> GenerateCEToken(Credentials credentials);
    Task<Result<string>> GeneratePortalToken(string portalUrl, string username, string password);
    Task<Result<ConfigResponse>> GetConfig(string url);
    void LogOut();
    string CEURL { get; set; }
    string PortalURL { get; set; }
}
