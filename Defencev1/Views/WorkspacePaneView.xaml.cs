using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.ViewModels;

namespace Defencev1.Views;

public partial class WorkspacePaneView : ContentView
{
	public WorkspacePaneView()
	{
		InitializeComponent();
        WeakReferenceMessenger.Default.Register<ControlPanelOptionSelectedMsg>(this, async (r, m) => await ToggleVisibility(m.Value));
    }

    private async Task ToggleVisibility(string value)
    {
        if (value == "workspaces")
        {
            IsVisible = true;
            if (BindingContext is WorkspaceViewModel vm)
                await vm.GetWorkspacesCommand.ExecuteAsync(null);
        }
        else
        {
            IsVisible = false;
        }
    }

    private void OnHideClicked(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ControlPanelOptionSelectedMsg("hide"));
    }
}