using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

// The PoC only implements Google sign-in on Android and iOS.
public sealed class UnsupportedAuthService : IAuthService
{
    public Task<AuthUser?> SignInWithGoogleAsync() =>
        throw new PlatformNotSupportedException("La prueba de login con Google solo está disponible en Android e iOS.");
}
