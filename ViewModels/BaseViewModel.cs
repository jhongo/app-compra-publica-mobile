using CommunityToolkit.Mvvm.ComponentModel;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>
/// Estado común de todas las pantallas: carga, título y error (docs/arquitectura.md, decisión 5).
/// La lógica de negocio va en los servicios, no aquí.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>Mensaje apto para el usuario. Nunca debe contener detalles técnicos.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}
