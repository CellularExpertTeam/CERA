using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;
using Defencev1.ViewModels;

namespace Defencev1.Views;

public partial class FilesPaneView : ContentView
{
    public FilesPaneView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ControlPanelOptionSelectedMsg>(this, (r, m) => ToggleVisibility(m.Value));
    }

    public void ToggleVisibility(string content)
    {
        if (IsVisible)
            IsVisible = !IsVisible;
        else
            IsVisible = content == "files";
    }
    private void OnHideClicked(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ControlPanelOptionSelectedMsg("files"));
    }

    private void OnLoadMmpkClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fullPath)
        {
            if (BindingContext is FilesPaneViewModel vm)
            {
                _ = vm.LoadMmpk(fullPath);
            }
        }
    }
}