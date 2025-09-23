using Defencev1.ViewModels.Base;

namespace Defencev1.Views;

public partial class ContentPageBase : ContentPage
{
    public ContentPageBase()
    {
        NavigationPage.SetBackButtonTitle(this, string.Empty);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not IViewModelBase viewModelBase)
        {
            return;
        }

        await viewModelBase.InitializeAsyncCommand.ExecuteAsync(null);
    }
}
