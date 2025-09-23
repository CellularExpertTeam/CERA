using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.ViewModels;
using Esri.ArcGISRuntime.Mapping;

namespace Defencev1.Views;

public partial class LayerPaneView : ContentView
{
	public LayerPaneView()
	{
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ControlPanelOptionSelectedMsg>(this, (r, m) => ToggleVisibility(m.Value));
    }

    public void ToggleVisibility(string optionName)
    {
        if (optionName == "layers")
        {
            IsVisible = true;
        }
        else
        {
            IsVisible = false;
        }
    }
    private void OnHideClicked(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new Messages.ControlPanelOptionSelectedMsg("hide"));
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.BindingContext is Layer layer)
        {   
            if (this.BindingContext is LayerViewModel vm)
                vm.RemoveLayer(layer);
        }
    }
}