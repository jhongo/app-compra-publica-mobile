using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Firebase.Auth;
using Foundation;
using Google.SignIn;

namespace CompraPublicCercaMobile.Services;

public sealed class IosAuthService : IAuthService
{
    private const nint GoogleSignInCancelledCode = -5;

    public async Task<AuthUser?> SignInWithGoogleAsync()
    {
        var clientId = Firebase.Core.App.DefaultInstance?.Options.ClientId
            ?? throw new InvalidOperationException("Falta Platforms/iOS/GoogleService-Info.plist. Revisa docs/firebase-auth.md.");
        var presenter = Platform.GetCurrentUIViewController()
            ?? throw new InvalidOperationException("No hay una pantalla activa.");

        var signIn = SignIn.SharedInstance;
        signIn.Configuration = new Google.SignIn.Configuration(clientId);

        // Always show the account picker instead of reusing the last account.
        signIn.SignOutUser();

        var signInResult = new TaskCompletionSource<SignInResult?>();
        signIn.SignInWithPresentingViewController(presenter, (result, error) =>
        {
            if (error is null)
            {
                signInResult.TrySetResult(result);
            }
            else if (error.Code == GoogleSignInCancelledCode)
            {
                signInResult.TrySetResult(null);
            }
            else
            {
                signInResult.TrySetException(new NSErrorException(error));
            }
        });

        var result = await signInResult.Task;
        if (result is null)
        {
            return null;
        }

        var idToken = result.User.IdToken?.TokenString
            ?? throw new InvalidOperationException("Google no devolvió un token.");
        var credential = GoogleAuthProvider.GetCredential(idToken, result.User.AccessToken.TokenString);
        var authResult = await Auth.DefaultInstance!.SignInWithCredentialAsync(credential);
        var user = authResult.User;

        return new AuthUser(user.Uid, user.DisplayName, user.Email);
    }

    public Task SignOutAsync()
    {
        SignIn.SharedInstance.SignOutUser();

        var auth = Auth.DefaultInstance;
        if (auth is not null && !auth.SignOut(out var error))
        {
            throw new NSErrorException(error);
        }

        return Task.CompletedTask;
    }
}
