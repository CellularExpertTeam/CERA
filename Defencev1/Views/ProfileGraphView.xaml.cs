using CommunityToolkit.Mvvm.Messaging;
using Defencev1.Messages;

namespace Defencev1.Views;

public partial class ProfileGraphView : ContentView
{
    public ProfileGraphView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ShowProfileGraphMsg>(
            this, (_, __) => IsVisible = true);
        WeakReferenceMessenger.Default.Register<CalculateProfileMsg>(this, (r, m) => IsVisible = true);
    }

    private void OnHideClicked(object sender, EventArgs e) => IsVisible = false;
}
