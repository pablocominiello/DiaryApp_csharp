using CommunityToolkit.Mvvm.ComponentModel;

namespace DiaryApp.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    // ✅ NUEVO: Propiedad calculada para binding inverso
    public bool IsNotBusy => !IsBusy;

    // ✅ NUEVO: Notificar cambios de IsNotBusy cuando IsBusy cambia
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
}