using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.Services;

public sealed class AccountService : IAccountService
{
    private const string BiometricEnabledKey = "biometric_login_enabled";
    private const string BiometricEmailKey = "biometric_login_email";

    private readonly IBackendAuthClient _api;
    private readonly IAuthService _google;
    private readonly ITokenStore _tokens;
    private readonly ISessionService _session;
    private readonly IUserLocationService _location;
    private readonly IBiometricService _biometrics;
    private readonly IPreferences _preferences;
    private readonly IDeviceInfo _device;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IBackendAuthClient api,
        IAuthService google,
        ITokenStore tokens,
        ISessionService session,
        IUserLocationService location,
        IBiometricService biometrics,
        IPreferences preferences,
        IDeviceInfo device,
        ILogger<AccountService> logger)
    {
        _api = api;
        _google = google;
        _tokens = tokens;
        _session = session;
        _location = location;
        _biometrics = biometrics;
        _preferences = preferences;
        _device = device;
        _logger = logger;
    }

    /// <summary>Nombre del token en la API: permite ver y revocar sesiones por dispositivo.</summary>
    private string DeviceName => $"{_device.Name} ({_device.Platform} {_device.VersionString})";

    public Task<int> SendVerificationCodeAsync(string email) =>
        _api.SendVerificationCodeAsync(Normalize(email));

    public Task<string> VerifyCodeAsync(string email, string code) =>
        _api.VerifyCodeAsync(Normalize(email), code.Trim());

    public async Task RegisterAsync(Registration registration)
    {
        var result = await _api.RegisterAsync(registration with { Email = Normalize(registration.Email) }, DeviceName);
        await OnSignedInAsync(result);
    }

    public Task<int> SendPasswordResetCodeAsync(string email) =>
        _api.SendPasswordResetCodeAsync(Normalize(email));

    public Task<string> VerifyPasswordResetCodeAsync(string email, string code) =>
        _api.VerifyPasswordResetCodeAsync(Normalize(email), code.Trim());

    public async Task ResetPasswordAsync(string email, string resetToken, string password, string passwordConfirmation)
    {
        var result = await _api.ResetPasswordAsync(Normalize(email), resetToken, password, passwordConfirmation, DeviceName);
        await OnSignedInAsync(result);
    }

    public async Task LoginAsync(string email, string password)
    {
        var result = await _api.LoginAsync(Normalize(email), password, DeviceName);
        await OnSignedInAsync(result);
    }

    public async Task<bool> LoginWithGoogleAsync()
    {
        var credential = await _google.SignInWithGoogleAsync();
        if (credential is null)
        {
            return false;
        }

        var result = await _api.LoginWithGoogleAsync(credential.FirebaseIdToken, DeviceName, _location.Province);
        await OnSignedInAsync(result);
        return true;
    }

    public async Task SignOutAsync()
    {
        var token = await _tokens.GetAsync();
        if (token is not null)
        {
            try
            {
                await _api.LogoutAsync(token);
            }
            catch (BackendApiException ex)
            {
                // Sin conexión o token ya vencido: igual se cierra la sesión en el teléfono.
                _logger.LogWarning(ex, "No se pudo revocar el token en la API.");
            }
        }

        try
        {
            await _google.SignOutAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cerrar la sesión de Google/Firebase.");
        }

        // Sin token ya no hay con qué entrar por biometría.
        _tokens.Clear();
        DisableBiometricLogin();
        _session.Clear();
    }

    public async Task SyncProvinceAsync(string province)
    {
        var token = await _tokens.GetAsync();
        if (!_session.IsAuthenticated || token is null)
        {
            return;
        }

        try
        {
            _session.SetUser(await _api.UpdateProvinceAsync(token, province));
        }
        catch (BackendApiException ex)
        {
            // La provincia ya quedó guardada en el teléfono; se reintentará en el próximo inicio de sesión.
            _logger.LogWarning(ex, "No se pudo guardar la provincia en la cuenta.");
        }
    }

    // ----- Biometría -----

    public BiometricKind BiometricKind => _biometrics.GetAvailableKind();

    public bool IsBiometricLoginEnabled => _preferences.Get(BiometricEnabledKey, false);

    public string? BiometricAccountEmail => _preferences.Get<string?>(BiometricEmailKey, null);

    public async Task<bool> CanLoginWithBiometricsAsync() =>
        IsBiometricLoginEnabled
        && BiometricKind != BiometricKind.None
        && await _tokens.GetAsync() is not null;

    public async Task<BiometricLoginResult> LoginWithBiometricsAsync()
    {
        var token = await _tokens.GetAsync();
        if (token is null || BiometricKind == BiometricKind.None)
        {
            DisableBiometricLogin();
            return BiometricLoginResult.Unavailable;
        }

        var check = await _biometrics.AuthenticateAsync("Ingresa a tu cuenta de Compra Pública Cerca");
        switch (check)
        {
            case BiometricResult.Cancelled:
                return BiometricLoginResult.Cancelled;
            case BiometricResult.Unavailable:
                return BiometricLoginResult.Unavailable;
            case BiometricResult.Failed:
                return BiometricLoginResult.Failed;
        }

        try
        {
            // El token puede haber vencido (30 días) o haberse revocado: la API lo confirma.
            var user = await _api.GetMeAsync(token);
            _session.SetUser(user);
            await SyncLocalProvinceAsync(token, user);
            return BiometricLoginResult.Success;
        }
        catch (BackendApiException ex) when (ex.IsUnauthorized)
        {
            _tokens.Clear();
            DisableBiometricLogin();
            return BiometricLoginResult.SessionExpired;
        }
        catch (BackendApiException ex)
        {
            _logger.LogWarning(ex, "No se pudo validar la sesión guardada.");
            return BiometricLoginResult.Failed;
        }
    }

    public async Task<BiometricResult> EnableBiometricLoginAsync()
    {
        if (_session.CurrentUser is not { } user)
        {
            return BiometricResult.Unavailable;
        }

        var result = await _biometrics.AuthenticateAsync($"Activa el inicio rápido con {BiometricKind.DisplayName()}");
        if (result == BiometricResult.Success)
        {
            _preferences.Set(BiometricEnabledKey, true);
            _preferences.Set(BiometricEmailKey, user.Email);
        }

        return result;
    }

    public void DisableBiometricLogin()
    {
        _preferences.Remove(BiometricEnabledKey);
        _preferences.Remove(BiometricEmailKey);
    }

    // ----- Privado -----

    private async Task OnSignedInAsync(AuthResult result)
    {
        // Revoca el token anterior de este teléfono (otra cuenta o sesión vieja) para no dejarlo activo.
        var previous = await _tokens.GetAsync();
        if (previous is not null && previous != result.Token)
        {
            _ = RevokeQuietlyAsync(previous);
        }

        await _tokens.SetAsync(result.Token);
        _session.SetUser(result.User);

        // El inicio rápido pertenece a una cuenta: si entra otra persona, se apaga.
        if (BiometricAccountEmail is { } email && !string.Equals(email, result.User.Email, StringComparison.OrdinalIgnoreCase))
        {
            DisableBiometricLogin();
        }

        await SyncLocalProvinceAsync(result.Token, result.User);
    }

    /// <summary>
    /// La provincia vive en la cuenta y en el teléfono. Gana la de la cuenta; si la
    /// cuenta aún no tiene (p. ej. registro con Google), se sube la del teléfono.
    /// </summary>
    private async Task SyncLocalProvinceAsync(string token, AuthUser user)
    {
        if (user.Province is { } accountProvince)
        {
            _location.SetProvince(accountProvince);
            return;
        }

        if (_location.Province is { } localProvince)
        {
            try
            {
                _session.SetUser(await _api.UpdateProvinceAsync(token, localProvince));
            }
            catch (BackendApiException ex)
            {
                _logger.LogWarning(ex, "No se pudo subir la provincia a la cuenta.");
            }
        }
    }

    private async Task RevokeQuietlyAsync(string token)
    {
        try
        {
            await _api.LogoutAsync(token);
        }
        catch (BackendApiException)
        {
            // Ya vencido o sin conexión: vencerá solo.
        }
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
