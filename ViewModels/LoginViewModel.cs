using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Mensaje informativo (no de error) que se muestra bajo el botón.</summary>
    [ObservableProperty]
    private string _statusMessage = "Inicia sesión para continuar.";

    public LoginViewModel(
        IAuthService authService,
        ISessionService session,
        INavigationService navigation,
        ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _session = session;
        _navigation = navigation;
        _logger = logger;
        Title = "Iniciar sesión";
    }

    // Los comandos async no permiten ejecuciones simultáneas: el botón se
    // deshabilita solo mientras el login está en curso.
    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Conectando con Google…";

        try
        {
            var user = await _authService.SignInWithGoogleAsync();
            if (user is null)
            {
                StatusMessage = "Inicio de sesión cancelado.";
                return;
            }

            _session.SetUser(user);
            StatusMessage = $"Hola, {user.DisplayName}";
            await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló el inicio de sesión con Google.");
            StatusMessage = string.Empty;
            ErrorMessage = "No se pudo iniciar sesión. Inténtalo de nuevo.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
