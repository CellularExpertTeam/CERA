using CommunityToolkit.Mvvm.ComponentModel;

namespace Defencev1.Models;

public partial class Credentials : ObservableObject
{
    [ObservableProperty]
    private string _portalUrl = string.Empty;
    [ObservableProperty]
    private string _password = string.Empty;
    [ObservableProperty]
    private string _userName = string.Empty;
    [ObservableProperty]
    private string _apiUrl = string.Empty;
    [ObservableProperty]
    private string _ceServerUrl = string.Empty;
}
