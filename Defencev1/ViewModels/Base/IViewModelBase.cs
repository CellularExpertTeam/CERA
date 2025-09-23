using CommunityToolkit.Mvvm.Input;

namespace Defencev1.ViewModels.Base;

public interface IViewModelBase : IQueryAttributable
{
    public IAsyncRelayCommand InitializeAsyncCommand { get; }

    public bool IsBusy { get; }

    public bool IsInitialized { get; }

    Task InitializeAsync();
}

