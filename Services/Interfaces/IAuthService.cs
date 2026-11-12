using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Returns the signed-in user, or null if the user cancelled.</summary>
    Task<AuthUser?> SignInWithGoogleAsync();

    /// <summary>Closes the Firebase and Google sessions on this device.</summary>
    Task SignOutAsync();
}
