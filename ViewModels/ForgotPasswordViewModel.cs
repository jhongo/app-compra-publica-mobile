using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>
/// Recuperar la contraseña en tres pasos, en una sola pantalla:
/// 1. correo → se envía un código, 2. código (se valida solo al completar los 6 dígitos),
/// 3. contraseña nueva → se guarda y se entra a la app.
/// </summary>
public partial class ForgotPasswordViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IAccountService _account;
    private readonly INavigationService _navigation;

    private CancellationTokenSource? _countdownCts;
    private string? _resetToken;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailStep), nameof(IsCodeStep), nameof(IsPasswordStep), nameof(Subtitle))]
    private ForgotPasswordStep _step = ForgotPasswordStep.Email;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _passwordConfirmation = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private string _emailError = string.Empty;

    [ObservableProperty]
    private string _codeError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private string _passwordConfirmationError = string.Empty;

    [ObservableProperty]
    private bool _isVerifyingCode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResendText), nameof(CanResend))]
    private int _resendSeconds;

    public ForgotPasswordViewModel(IAccountService account, INavigationService navigation)
    {
        _account = account;
        _navigation = navigation;
        Title = "Recuperar contraseña";
    }

    public bool IsEmailStep => Step == ForgotPasswordStep.Email;

    public bool IsCodeStep => Step == ForgotPasswordStep.Code;

    public bool IsPasswordStep => Step == ForgotPasswordStep.NewPassword;

    public string Subtitle => Step switch
    {
        ForgotPasswordStep.Code => $"Escribe el código de 6 dígitos que enviamos a {Email.Trim()}. Si no lo ves, revisa la carpeta de spam.",
        ForgotPasswordStep.NewPassword => "Elige una contraseña nueva. Cerraremos tu sesión en los demás dispositivos.",
        _ => "Escribe el correo de tu cuenta y te enviaremos un código para crear una contraseña nueva."
    };

    public string PasswordHint => InputRules.PasswordHint;

    public string ResendText => ResendSeconds > 0 ? $"Reenviar código ({ResendSeconds})" : "Reenviar código";

    public bool CanResend => ResendSeconds == 0;

    /// <summary>El login pasa el correo que ya se escribió, para no pedirlo dos veces.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavigationRoutes.EmailParameter, out var value) && value is string email)
        {
            Email = Uri.UnescapeDataString(email);
        }
    }

    partial void OnEmailChanged(string value) => EmailError = string.Empty;

    partial void OnPasswordChanged(string value) => PasswordError = string.Empty;

    partial void OnPasswordConfirmationChanged(string value) => PasswordConfirmationError = string.Empty;

    /// <summary>Al completar los 6 dígitos se valida solo.</summary>
    partial void OnCodeChanged(string value)
    {
        CodeError = string.Empty;
        if (IsCodeStep && value.Trim().Length == BackendConstants.VerificationCodeLength)
        {
            VerifyCodeCommand.Execute(null);
        }
    }

    /// <summary>Volver: en el paso 2 regresa a corregir el correo; en los demás, sale de la pantalla.</summary>
    [RelayCommand]
    private Task GoBackAsync()
    {
        if (IsCodeStep)
        {
            ChangeEmail();
            return Task.CompletedTask;
        }

        return _navigation.GoToAsync("..");
    }

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!InputRules.IsValidEmail(Email))
        {
            EmailError = "Escribe un correo electrónico válido.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var resendIn = await _account.SendPasswordResetCodeAsync(Email);
            Code = string.Empty;
            Step = ForgotPasswordStep.Code;
            StartCountdown(resendIn);
        }
        catch (BackendApiException ex)
        {
            if (IsEmailStep)
            {
                EmailError = ex.ErrorFor("email") ?? ex.Message;
            }
            else
            {
                CodeError = ex.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResendCodeAsync()
    {
        if (CanResend)
        {
            await SendCodeAsync();
        }
    }

    [RelayCommand]
    private void ChangeEmail()
    {
        _countdownCts?.Cancel();
        ResendSeconds = 0;
        Code = string.Empty;
        CodeError = string.Empty;
        Step = ForgotPasswordStep.Email;
    }

    [RelayCommand]
    private async Task VerifyCodeAsync()
    {
        if (IsVerifyingCode)
        {
            return;
        }

        IsVerifyingCode = true;
        try
        {
            _resetToken = await _account.VerifyPasswordResetCodeAsync(Email, Code);
            _countdownCts?.Cancel();
            Step = ForgotPasswordStep.NewPassword;
        }
        catch (BackendApiException ex)
        {
            CodeError = ex.ErrorFor("code") ?? ex.Message;
        }
        finally
        {
            IsVerifyingCode = false;
        }
    }

    [RelayCommand]
    private async Task SavePasswordAsync()
    {
        if (IsBusy || !ValidatePassword())
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _account.ResetPasswordAsync(Email, _resetToken!, Password, PasswordConfirmation);
            await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
        }
        catch (BackendApiException ex)
        {
            PasswordError = ex.ErrorFor("password") ?? string.Empty;

            // El permiso venció (30 min) o ya se usó: hay que pedir otro código.
            if (ex.ErrorFor("reset_token") is { } tokenError)
            {
                _resetToken = null;
                Code = string.Empty;
                Step = ForgotPasswordStep.Code;
                CodeError = tokenError;
                return;
            }

            ErrorMessage = ex.FieldErrors.Count > 0 ? string.Empty : ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidatePassword()
    {
        PasswordError = InputRules.PasswordError(Password) ?? string.Empty;
        PasswordConfirmationError = PasswordError.Length == 0 && Password != PasswordConfirmation
            ? "Las contraseñas no coinciden."
            : string.Empty;
        return PasswordError.Length == 0 && PasswordConfirmationError.Length == 0;
    }

    /// <summary>Cuenta regresiva para reenviar; la API acepta un envío por minuto.</summary>
    private async void StartCountdown(int seconds)
    {
        _countdownCts?.Cancel();
        var cts = _countdownCts = new CancellationTokenSource();
        try
        {
            for (ResendSeconds = seconds; ResendSeconds > 0; ResendSeconds--)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Se validó el código o se cambió el correo.
        }
    }
}

public enum ForgotPasswordStep
{
    Email,
    Code,
    NewPassword
}
