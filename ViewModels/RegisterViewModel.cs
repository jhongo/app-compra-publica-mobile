using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>
/// Registro manual. El correo se valida antes de registrar:
/// «Validar» envía un código → al escribir los 6 dígitos se comprueba solo → ✓.
/// </summary>
public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAccountService _account;
    private readonly INavigationService _navigation;
    private readonly IUserLocationService _userLocation;
    private readonly ILogger<RegisterViewModel> _logger;

    private CancellationTokenSource? _countdownCts;
    private string? _verificationToken;
    private bool _birthDateChosen;

    // ----- Campos -----

    [ObservableProperty]
    private string _firstNames = string.Empty;

    [ObservableProperty]
    private string _lastNames = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _verificationCode = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _passwordConfirmation = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private string _address = string.Empty;

    /// <summary>Provincia donde busca oportunidades. Se puede cambiar después desde Inicio.</summary>
    [ObservableProperty]
    private string? _province;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private DateTime _birthDate = new(2000, 1, 1);

    // ----- Errores por campo (vacío = sin error) -----

    [ObservableProperty]
    private string _firstNamesError = string.Empty;

    [ObservableProperty]
    private string _lastNamesError = string.Empty;

    [ObservableProperty]
    private string _emailError = string.Empty;

    [ObservableProperty]
    private string _codeError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private string _passwordConfirmationError = string.Empty;

    [ObservableProperty]
    private string _provinceError = string.Empty;

    [ObservableProperty]
    private string _postalCodeError = string.Empty;

    [ObservableProperty]
    private string _birthDateError = string.Empty;

    // ----- Validación del correo -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValidateButtonText), nameof(CanRequestCode))]
    private bool _isSendingCode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValidateButtonText), nameof(CanRequestCode), nameof(IsCodeFieldVisible))]
    private bool _isCodeSent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValidateButtonText), nameof(CanRequestCode), nameof(IsCodeFieldVisible))]
    private bool _isEmailVerified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValidateButtonText), nameof(CanRequestCode))]
    private int _resendSeconds;

    [ObservableProperty]
    private bool _isVerifyingCode;

    /// <summary>Error del registro con Google: se muestra junto a su botón, al final del formulario.</summary>
    [ObservableProperty]
    private string _googleErrorMessage = string.Empty;

    /// <summary>Texto informativo bajo el código: a dónde se envió o que ya se validó.</summary>
    [ObservableProperty]
    private string _codeHint = string.Empty;

    public RegisterViewModel(
        IAccountService account,
        INavigationService navigation,
        IUserLocationService userLocation,
        ILogger<RegisterViewModel> logger)
    {
        _account = account;
        _navigation = navigation;
        _userLocation = userLocation;
        _logger = logger;
        Title = "Registro";
        Province = userLocation.Province;
    }

    public IReadOnlyList<string> Provinces => EcuadorProvinces.All;

    /// <summary>Solo se permiten fechas de nacimiento hasta ayer.</summary>
    public DateTime MaximumBirthDate { get; } = DateTime.Today.AddDays(-1);

    public string PasswordHint => InputRules.PasswordHint;

    public string ValidateButtonText => this switch
    {
        { IsEmailVerified: true } => "Validado",
        { IsSendingCode: true } => "Enviando…",
        { ResendSeconds: > 0 } => $"Reenviar ({ResendSeconds})",
        { IsCodeSent: true } => "Reenviar",
        _ => "Validar"
    };

    public bool CanRequestCode => !IsEmailVerified && !IsSendingCode && ResendSeconds == 0;

    /// <summary>El campo del código aparece cuando ya se envió y aún no se valida.</summary>
    public bool IsCodeFieldVisible => IsCodeSent && !IsEmailVerified;

    // Al escribir, se limpia el error de ese campo.
    partial void OnFirstNamesChanged(string value) => FirstNamesError = string.Empty;

    partial void OnLastNamesChanged(string value) => LastNamesError = string.Empty;

    partial void OnPasswordChanged(string value) => PasswordError = string.Empty;

    partial void OnPasswordConfirmationChanged(string value) => PasswordConfirmationError = string.Empty;

    partial void OnProvinceChanged(string? value) => ProvinceError = string.Empty;

    partial void OnPostalCodeChanged(string value) => PostalCodeError = string.Empty;

    partial void OnBirthDateChanged(DateTime value)
    {
        _birthDateChosen = true;
        BirthDateError = string.Empty;
    }

    /// <summary>Si cambia el correo, el código o la validación anteriores ya no sirven.</summary>
    partial void OnEmailChanged(string value)
    {
        EmailError = string.Empty;
        if (IsCodeSent || IsEmailVerified)
        {
            ResetEmailVerification();
        }
    }

    /// <summary>Al completar los 6 dígitos se valida solo, sin otro botón.</summary>
    partial void OnVerificationCodeChanged(string value)
    {
        CodeError = string.Empty;
        if (value.Trim().Length == BackendConstants.VerificationCodeLength && IsCodeSent && !IsEmailVerified)
        {
            VerifyCodeCommand.Execute(null);
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigation.GoToAsync("..");

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        if (!CanRequestCode)
        {
            return;
        }

        if (!InputRules.IsValidEmail(Email))
        {
            EmailError = "Escribe un correo electrónico válido.";
            return;
        }

        IsSendingCode = true;
        EmailError = string.Empty;
        CodeError = string.Empty;
        try
        {
            var resendIn = await _account.SendVerificationCodeAsync(Email);
            IsCodeSent = true;
            VerificationCode = string.Empty;
            CodeHint = $"Te enviamos un código a {Email.Trim()}. Revisa también la carpeta de spam.";
            StartCountdown(resendIn);
        }
        catch (BackendApiException ex)
        {
            EmailError = ex.ErrorFor("email") ?? ex.Message;
        }
        finally
        {
            IsSendingCode = false;
        }
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
            _verificationToken = await _account.VerifyCodeAsync(Email, VerificationCode);
            IsEmailVerified = true;
            CodeError = string.Empty;
            CodeHint = "Correo validado.";
            _countdownCts?.Cancel();
            ResendSeconds = 0;
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
    private async Task RegisterAsync()
    {
        if (IsBusy || !Validate())
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _account.RegisterAsync(new Registration(
                FirstNames.Trim(),
                LastNames.Trim(),
                Email,
                Password,
                PasswordConfirmation,
                _verificationToken!,
                Province!,
                Address,
                PostalCode,
                _birthDateChosen ? BirthDate : null));

            await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
        }
        catch (BackendApiException ex)
        {
            ShowServerErrors(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterWithGoogleAsync()
    {
        if (IsBusy)
        {
            return;
        }

        // Si ya eligió provincia en el formulario, la cuenta de Google se crea con ella.
        if (!string.IsNullOrEmpty(Province))
        {
            _userLocation.SetProvince(Province);
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        GoogleErrorMessage = string.Empty;
        try
        {
            if (await _account.LoginWithGoogleAsync())
            {
                await _navigation.GoToAsync($"//{NavigationRoutes.Inicio}");
            }
        }
        catch (BackendApiException ex)
        {
            GoogleErrorMessage = ex.Message;
        }
        catch (GoogleSignInNotConfiguredException ex)
        {
            _logger.LogWarning(ex, "Google no está configurado.");
            GoogleErrorMessage = GoogleSignInNotConfiguredException.UserMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló el registro con Google.");
            GoogleErrorMessage = "No se pudo continuar con Google. Inténtalo de nuevo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Revisa en el teléfono lo mismo que revisa la API, para avisar sin esperar a la red.</summary>
    private bool Validate()
    {
        FirstNamesError = string.IsNullOrWhiteSpace(FirstNames) ? "Escribe tus nombres." : string.Empty;
        LastNamesError = string.IsNullOrWhiteSpace(LastNames) ? "Escribe tus apellidos." : string.Empty;
        EmailError = InputRules.IsValidEmail(Email) ? string.Empty : "Escribe un correo electrónico válido.";
        CodeError = IsEmailVerified || EmailError.Length > 0
            ? string.Empty
            : IsCodeSent ? "Escribe el código que te enviamos." : "Toca «Validar» para confirmar tu correo.";
        PasswordError = InputRules.PasswordError(Password) ?? string.Empty;
        PasswordConfirmationError = PasswordError.Length == 0 && Password != PasswordConfirmation
            ? "Las contraseñas no coinciden."
            : string.Empty;
        ProvinceError = string.IsNullOrEmpty(Province) ? "Elige tu provincia." : string.Empty;
        PostalCodeError = InputRules.IsValidPostalCode(PostalCode) ? string.Empty : "El código postal tiene 6 dígitos.";

        var errors = new[]
        {
            FirstNamesError, LastNamesError, EmailError, CodeError, PasswordError,
            PasswordConfirmationError, ProvinceError, PostalCodeError
        };
        var valid = errors.All(e => e.Length == 0);
        ErrorMessage = valid ? string.Empty : "Revisa los campos marcados.";
        return valid;
    }

    private void ShowServerErrors(BackendApiException ex)
    {
        FirstNamesError = ex.ErrorFor("first_names") ?? string.Empty;
        LastNamesError = ex.ErrorFor("last_names") ?? string.Empty;
        EmailError = ex.ErrorFor("email") ?? string.Empty;
        PasswordError = ex.ErrorFor("password") ?? string.Empty;
        ProvinceError = ex.ErrorFor("province") ?? string.Empty;
        PostalCodeError = ex.ErrorFor("postal_code") ?? string.Empty;
        BirthDateError = ex.ErrorFor("birth_date") ?? string.Empty;

        // El token de verificación venció (30 min) o ya se usó: hay que validar de nuevo.
        if (ex.ErrorFor("verification_token") is { } tokenError)
        {
            ResetEmailVerification();
            CodeError = tokenError;
        }

        ErrorMessage = ex.FieldErrors.Count > 0 ? "Revisa los campos marcados." : ex.Message;
    }

    private void ResetEmailVerification()
    {
        _countdownCts?.Cancel();
        _verificationToken = null;
        IsCodeSent = false;
        IsEmailVerified = false;
        ResendSeconds = 0;
        VerificationCode = string.Empty;
        CodeHint = string.Empty;
    }

    /// <summary>Cuenta regresiva para «Reenviar»; la API solo acepta un envío por minuto.</summary>
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
            // Se validó el correo o se cambió: la cuenta regresiva ya no aplica.
        }
    }
}
