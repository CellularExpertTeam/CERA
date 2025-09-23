using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Enums;
using Defencev1.Messages;
using Defencev1.ViewModels.Predictions;

namespace Defencev1.Views;

public partial class QuickPredictionView : ContentView
{
    public QuickPredictionView()
	{
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ControlPanelToolSelectedMsg>(this, async (r, m) => await ToggleVisibility(m.Value));
	}

    private async Task ToggleVisibility(Tools tool)
    {
        if (tool == Tools.QuickPrediction)
        {
            IsVisible = true;
            if (BindingContext is QuickPredictionViewModel vm)
                await vm.GetPredictionModels();
        }
        else
        {
            IsVisible = false;
        }
        
    }
    private void OnHideClicked(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new Messages.ControlPanelToolSelectedMsg(Tools.None));
    }
}