namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Inicio de sesión con Google a través de Firebase (implementación por plataforma).
/// No crea la sesión de la app: entrega el ID token de Firebase para que la API lo valide.
/// </summary>
public interface IAuthService
{
    /// <summary>Returns the Firebase credential, or null if the user cancelled.</summary>
    Task<GoogleCredential?> SignInWithGoogleAsync();

    /// <summary>Closes the Firebase and Google sessions on this device.</summary>
    Task SignOutAsync();
}

/// <param name="FirebaseIdToken">JWT firmado por Firebase; la API lo valida con las claves públicas de Google.</param>
public sealed record GoogleCredential(string FirebaseIdToken, string? DisplayName, string? Email);
