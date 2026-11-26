using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Cuenta del usuario: registro, inicio y cierre de sesión, e inicio rápido con biometría.
/// Coordina la API, el almacenamiento seguro del token y la sesión en memoria.
/// Los errores de la API llegan como BackendApiException con el mensaje para el usuario.
/// </summary>
public interface IAccountService
{
    /// <returns>Segundos antes de poder reenviar el código.</returns>
    Task<int> SendVerificationCodeAsync(string email);

    /// <returns>Token de verificación para el registro.</returns>
    Task<string> VerifyCodeAsync(string email, string code);

    Task RegisterAsync(Registration registration);

    // ----- Recuperar contraseña -----

    /// <returns>Segundos antes de poder reenviar el código.</returns>
    Task<int> SendPasswordResetCodeAsync(string email);

    /// <returns>Token para cambiar la contraseña.</returns>
    Task<string> VerifyPasswordResetCodeAsync(string email, string code);

    /// <summary>Cambia la contraseña e inicia sesión (las demás sesiones de la cuenta se cierran).</summary>
    Task ResetPasswordAsync(string email, string resetToken, string password, string passwordConfirmation);

    Task LoginAsync(string email, string password);

    /// <returns>False si el usuario canceló la ventana de Google.</returns>
    Task<bool> LoginWithGoogleAsync();

    Task SignOutAsync();

    /// <summary>Guarda la provincia en la cuenta (si hay sesión). Los errores se registran y se ignoran.</summary>
    Task SyncProvinceAsync(string province);

    // ----- Inicio rápido con biometría -----

    /// <summary>Biometría disponible en este teléfono (None si no tiene o no está configurada).</summary>
    BiometricKind BiometricKind { get; }

    bool IsBiometricLoginEnabled { get; }

    /// <summary>Correo de la cuenta con la que se activó el inicio rápido.</summary>
    string? BiometricAccountEmail { get; }

    /// <summary>True si está activado, el teléfono lo soporta y hay un token guardado.</summary>
    Task<bool> CanLoginWithBiometricsAsync();

    Task<BiometricLoginResult> LoginWithBiometricsAsync();

    /// <summary>Pide la biometría una vez para confirmar y activa el inicio rápido.</summary>
    Task<BiometricResult> EnableBiometricLoginAsync();

    /// <summary>Se activa solo desde el interruptor de Perfil; la app nunca lo ofrece ni lo lanza sola.</summary>
    void DisableBiometricLogin();
}

public enum BiometricLoginResult
{
    Success,
    Cancelled,
    /// <summary>El token guardado venció o fue revocado: se desactiva el inicio rápido.</summary>
    SessionExpired,
    Unavailable,
    /// <summary>No se pudo contactar a la API; el inicio rápido sigue activo.</summary>
    Failed
}
