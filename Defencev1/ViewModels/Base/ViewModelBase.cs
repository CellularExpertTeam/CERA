using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Defencev1.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
{
    private long _isBusy;

    [ObservableProperty] private bool _isInitialized;

    public ViewModelBase()
    {
        InitializeAsyncCommand =
            new AsyncRelayCommand(
                async () =>
                {
                    await IsBusyFor(InitializeAsync);
                    IsInitialized = true;
                },
                AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler);
    }

    public bool IsBusy => Interlocked.Read(ref _isBusy) > 0;
    public IAsyncRelayCommand InitializeAsyncCommand { get; }

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
    {
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task IsBusyFor(Func<Task> unitOfWork)
    {
        Interlocked.Increment(ref _isBusy);
        OnPropertyChanged(nameof(IsBusy));

        try
        {
            await unitOfWork();
        }
        finally
        {
            Interlocked.Decrement(ref _isBusy);
            OnPropertyChanged(nameof(IsBusy));
        }
    }
}
