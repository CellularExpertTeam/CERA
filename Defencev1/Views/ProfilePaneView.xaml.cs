using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Enums;
using Defencev1.Messages;

namespace Defencev1.Views;

public partial class ProfilePaneView : ContentView
{
    public ProfilePaneView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ControlPanelToolSelectedMsg>(this, (r, m) => ToggleVisibility(m.Value));
        WeakReferenceMessenger.Default.Register<CalculateProfileMsg>(this, (r, m) => ProfileResults.IsVisible = true);
    }

    private void ToggleVisibility(Tools tool)
    {
        if (tool == Tools.Profile)
        {
            IsVisible = true;
            WeakReferenceMessenger.Default.Send(new ProfileToolStatusMsg(true));
        }
        else
        {
            IsVisible = false;
            WeakReferenceMessenger.Default.Send(new ProfileToolStatusMsg(false));
        }
    }
    private void OnHideClicked(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ControlPanelToolSelectedMsg(Tools.None));
    }

    private void ShowChart(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ShowProfileGraphMsg());
    }

}