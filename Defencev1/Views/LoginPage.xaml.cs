using Defencev1.Enums;
using Defencev1.ViewModels;

namespace Defencev1.Views;

public partial class LoginPage : ContentPage
{
	private readonly LoginViewModel _loginViewModel;
	private Color _bg;
	private Color _bg2;
	public LoginPage(LoginViewModel loginVm)
	{
        InitializeComponent();
		if (Application.Current.Resources.TryGetValue("Background", out object resourceBg))
		{
			if (resourceBg is Color color)
				_bg = color;
		}

		if (Application.Current.Resources.TryGetValue("Background2", out object resourceBg2))
		{
			if (resourceBg2 is Color color2)
				_bg2 = color2;
		}

        _loginViewModel = loginVm;
		BindingContext = _loginViewModel;
    }

	public void OnlineCredentialsClicked(object sender, EventArgs e)
	{
		OnlinePanelButton.BackgroundColor = _bg2;
		LocalPanelButton.BackgroundColor = _bg;

		if (BindingContext is  LoginViewModel loginVm)
		{
			loginVm.SelectedOperatingMode = AppOperatingMode.OnlineServices;
		}
	}

	public void LocalCredentialsClicked(object sender, EventArgs e)
	{
		OnlinePanelButton.BackgroundColor = _bg;
		LocalPanelButton.BackgroundColor = _bg2;

        if (BindingContext is LoginViewModel loginVm)
        {
            loginVm.SelectedOperatingMode = AppOperatingMode.LocalServices;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel loginVm)
        {
			loginVm.ResultMsg = string.Empty;
        }
    }
}