using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>Llamadas HTTP a la API propia. Todas lanzan BackendApiException si fallan.</summary>
public interface IBackendAuthClient
{
    /// <returns>Segundos que hay que esperar para reenviar el código.</returns>
    Task<int> SendVerificationCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <returns>Token de un solo uso para enviar en el registro.</returns>
    Task<string> VerifyCodeAsync(string email, string code, CancellationToken cancellationToken = default);

    /// <summary>Recuperar contraseña. La API responde igual exista o no la cuenta.</summary>
    /// <returns>Segundos que hay que esperar para reenviar el código.</returns>
    Task<int> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <returns>Token de un solo uso para cambiar la contraseña.</returns>
    Task<string> VerifyPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);

    /// <summary>Guarda la contraseña nueva, cierra las demás sesiones e inicia sesión en este teléfono.</summary>
    Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string password, string passwordConfirmation, string deviceName, CancellationToken cancellationToken = default);

    Task<AuthResult> RegisterAsync(Registration registration, string deviceName, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(string email, string password, string deviceName, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginWithGoogleAsync(string firebaseIdToken, string deviceName, string? province, CancellationToken cancellationToken = default);

    Task<AuthUser> GetMeAsync(string token, CancellationToken cancellationToken = default);

    Task<AuthUser> UpdateProvinceAsync(string token, string province, CancellationToken cancellationToken = default);

    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>Resultado de registro o login: el token de la API y el usuario.</summary>
public sealed record AuthResult(string Token, AuthUser User, bool IsNewUser);
