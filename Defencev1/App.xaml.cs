
using CommunityToolkit.Maui.Behaviors;
using Defencev1.Views;

namespace Defencev1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Current!.UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
    
}
