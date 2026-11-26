namespace CompraPublicCercaMobile.Services;

/// <summary>
/// Falta el archivo de Firebase de la plataforma (GoogleService-Info.plist en iOS,
/// google-services.json en Android). No se suben a git: ver docs/firebase-auth.md.
/// </summary>
public sealed class GoogleSignInNotConfiguredException(string missingFile)
    : InvalidOperationException($"Falta {missingFile}. Revisa docs/firebase-auth.md.")
{
    /// <summary>Mensaje para el usuario (sin rutas de archivos).</summary>
    public const string UserMessage = "El inicio con Google todavía no está configurado en esta versión de la app.";
}
