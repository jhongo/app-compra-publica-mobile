using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private const string DefaultDisplayName = "Proveedor";
    private const string DefaultEmail = "Sin correo registrado";

    private readonly IAccountService _account;
    private readonly ISessionService _session;
    private readonly IUserLocationService _userLocation;
    private readonly IFavoritesService _favorites;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;
    private readonly ILogger<ProfileViewModel> _logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initial))]
    private string _displayName = DefaultDisplayName;

    [ObservableProperty]
    private string _email = DefaultEmail;

    /// <summary>Provincia elegida, o el texto que invita a elegirla.</summary>
    [ObservableProperty]
    private string _provinceText = string.Empty;

    [ObservableProperty]
    private string _favoritesText = string.Empty;

    /// <summary>El teléfono tiene Face ID / huella configurados.</summary>
    [ObservableProperty]
    private bool _isBiometricAvailable;

    [ObservableProperty]
    private string _biometricText = string.Empty;

    /// <summary>Interruptor de «Inicio rápido». Al encenderlo se pide la biometría para confirmar.</summary>
    [ObservableProperty]
    private bool _isBiometricEnabled;

    /// <summary>Evita reaccionar a los cambios del interruptor hechos por código.</summary>
    private bool _isSyncingBiometricSwitch;

    public ProfileViewModel(
        IAccountService account,
        ISessionService session,
        IUserLocationService userLocation,
        IFavoritesService favorites,
        INavigationService navigation,
        IDialogService dialogs,
        IAppInfo appInfo,
        ILogger<ProfileViewModel> logger)
    {
        _account = account;
        _session = session;
        _userLocation = userLocation;
        _favorites = favorites;
        _navigation = navigation;
        _dialogs = dialogs;
        _logger = logger;
        Title = "Mi perfil";
        VersionText = $"Versión {appInfo.VersionString}";
    }

    /// <summary>Inicial del nombre para el avatar (no hay foto de perfil).</summary>
    public string Initial => DisplayName[..1].ToUpperInvariant();

    public string VersionText { get; }

    /// <summary>Lee usuario, provincia y favoritos. Se llama cada vez que se muestra la pantalla.</summary>
    public void LoadProfile()
    {
        var user = _session.CurrentUser;
        DisplayName = string.IsNullOrWhiteSpace(user?.DisplayName) ? DefaultDisplayName : user.DisplayName;
        Email = string.IsNullOrWhiteSpace(user?.Email) ? DefaultEmail : user.Email;
        ProvinceText = _userLocation.Province ?? "Elegir";
        FavoritesText = _favorites.Count.ToString();

        var kind = _account.BiometricKind;
        IsBiometricAvailable = kind != BiometricKind.None;
        BiometricText = $"Inicio rápido con {kind.DisplayName()}";
        SetBiometricSwitch(_account.IsBiometricLoginEnabled);
    }

    partial void OnIsBiometricEnabledChanged(bool value)
    {
        if (!_isSyncingBiometricSwitch)
        {
            _ = ToggleBiometricLoginAsync(value);
        }
    }

    private async Task ToggleBiometricLoginAsync(bool enable)
    {
        if (!enable)
        {
            _account.DisableBiometricLogin();
            return;
        }

        var result = await _account.EnableBiometricLoginAsync();
        if (result != BiometricResult.Success)
        {
            // Canceló o falló: el interruptor vuelve a apagado.
            SetBiometricSwitch(false);
        }
    }

    private void SetBiometricSwitch(bool value)
    {
        _isSyncingBiometricSwitch = true;
        IsBiometricEnabled = value;
        _isSyncingBiometricSwitch = false;
    }

    [RelayCommand]
    private Task OpenFavoritesAsync() => _navigation.GoToAsync($"//{NavigationRoutes.Favoritos}");

    [RelayCommand]
    private Task ChangeLocationAsync() => _navigation.GoToAsync(NavigationRoutes.LocationPicker);

    [RelayCommand]
    private async Task SignOutAsync()
    {
        if (IsBusy)
        {
            return;
        }

        // Acción que saca al usuario de la app: se confirma antes (Apple HIG / Material).
        var confirmed = await _dialogs.ConfirmAsync(
            "¿Cerrar sesión?",
            "Tus favoritos se mantienen en este dispositivo. El inicio rápido se desactivará hasta que vuelvas a entrar.",
            "Cerrar sesión",
            "Cancelar");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        try
        {
            // Revoca el token en la API y borra todo lo de la sesión en el teléfono.
            await _account.SignOutAsync();
        }
        catch (Exception ex)
        {
            // SignOutAsync ya tolera fallas de red; esto cubre lo inesperado.
            _logger.LogError(ex, "Falló el cierre de sesión.");
            _session.Clear();
        }
        finally
        {
            IsBusy = false;
        }

        await _navigation.GoToAsync($"//{NavigationRoutes.Login}");
    }
}
