using Defencev1.Models.ProfileModels;
using Defencev1.Utils.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Services.Profile;

public interface IProfileService
{
    Task<Result<ProfileResponse>> PostProfile(long workspaceId, ProfileRequest profileRequest);
    ProfileResponse? CurrentProfile { get; }
}
