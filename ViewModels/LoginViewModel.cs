using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAccountService _account;
    private readonly INavigationService _navigation;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Mensaje informativo (no de error) que se muestra bajo el botón.</summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private string _emailError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private bool _canUseBiometrics;

    [ObservableProperty]
    private string _biometricButtonText = string.Empty;

    [ObservableProperty]
    private string _biometricAccountText = string.Empty;

    public LoginViewModel(
        IAccountService account,
        INavigationService navigation,
        ILogger<LoginViewModel> logger)
    {
        _account = account;
        _navigation = navigation;
        _logger = logger;
        Title = "Iniciar sesión";
    }

    /// <summary>
    /// Muestra el botón «Ingresar con Face ID / huella» si se activó en Perfil.
    /// No lanza la biometría sola: el usuario decide cuándo usarla.
    /// </summary>
    public async Task OnAppearingAsync()
    {
        CanUseBiometrics = await _account.CanLoginWithBiometricsAsync();
        if (!CanUseBiometrics)
        {
            return;
        }

        BiometricButtonText = $"Ingresar con {_account.BiometricKind.DisplayName()}";
        BiometricAccountText = _account.BiometricAccountEmail ?? string.Empty;
    }

    partial void OnEmailChanged(string value) => EmailError = string.Empty;

    partial void OnPasswordChanged(string value) => PasswordError = string.Empty;

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy || !Validate())
        {
            return;
        }

        await RunAsync(async () =>
        {
            await _account.LoginAsync(Email, Password);
            await EnterAppAsync();
        });
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

        StatusMessage = "Conectando con Google…";
        await RunAsync(async () =>
        {
            if (!await _account.LoginWithGoogleAsync())
            {
                StatusMessage = "Inicio de sesión cancelado.";
                return;
            }

            await EnterAppAsync();
        });
    }

    [RelayCommand]
    private async Task LoginWithBiometricsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            switch (await _account.LoginWithBiometricsAsync())
            {
                case BiometricLoginResult.Success:
                    await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
                    break;
                case BiometricLoginResult.SessionExpired:
                    CanUseBiometrics = false;
                    ErrorMessage = "Tu sesión venció. Inicia sesión con tu contraseña para volver a activar el inicio rápido.";
                    break;
                case BiometricLoginResult.Unavailable:
                    CanUseBiometrics = await _account.CanLoginWithBiometricsAsync();
                    ErrorMessage = "La biometría no está disponible. Inicia sesión con tu contraseña.";
                    break;
                case BiometricLoginResult.Failed:
                    ErrorMessage = "No pudimos validar tu sesión. Revisa tu conexión e inténtalo de nuevo.";
                    break;
                // Cancelled: el usuario eligió escribir su contraseña; no es un error.
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoToRegisterAsync() => _navigation.GoToAsync(NavigationRoutes.Registro);

    /// <summary>Lleva el correo ya escrito para no pedirlo de nuevo.</summary>
    [RelayCommand]
    private Task GoToForgotPasswordAsync() => _navigation.GoToAsync(
        NavigationRoutes.RecuperarContrasena,
        new Dictionary<string, object> { [NavigationRoutes.EmailParameter] = Email.Trim() });

    private bool Validate()
    {
        EmailError = InputRules.IsValidEmail(Email) ? string.Empty : "Escribe un correo electrónico válido.";
        PasswordError = string.IsNullOrEmpty(Password) ? "Escribe tu contraseña." : string.Empty;
        return EmailError.Length == 0 && PasswordError.Length == 0;
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await action();
        }
        catch (BackendApiException ex)
        {
            StatusMessage = string.Empty;
            EmailError = ex.ErrorFor("email") ?? string.Empty;
            PasswordError = ex.ErrorFor("password") ?? string.Empty;
            if (EmailError.Length == 0 && PasswordError.Length == 0)
            {
                ErrorMessage = ex.Message;
            }
        }
        catch (GoogleSignInNotConfiguredException ex)
        {
            _logger.LogWarning(ex, "Google no está configurado.");
            StatusMessage = string.Empty;
            ErrorMessage = GoogleSignInNotConfiguredException.UserMessage;
        }
        catch (Exception ex)
        {
            // Errores de Google/Firebase (configuración, red): detalle solo en el log.
            _logger.LogError(ex, "Falló el inicio de sesión.");
            StatusMessage = string.Empty;
            ErrorMessage = "No se pudo iniciar sesión. Inténtalo de nuevo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnterAppAsync()
    {
        Password = string.Empty;
        StatusMessage = string.Empty;
        await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
    }
}
