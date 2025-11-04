using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;

namespace Defencev1.Views;

public partial class SettingsPaneView : ContentView
{
	public SettingsPaneView()
	{
		InitializeComponent();
		WeakReferenceMessenger.Default.Register<ControlPanelOptionSelectedMsg>(this, (r, m) => ToggleVisibility(m.Value));
    }

	public void ToggleVisibility(string optionName)
	{
        if (IsVisible)
            IsVisible = !IsVisible;
        else
            IsVisible = optionName == "settings";
    }
	private void OnHideClicked(object sender, EventArgs e)
	{
		WeakReferenceMessenger.Default.Send(new Messages.ControlPanelOptionSelectedMsg("hide"));
    }
}