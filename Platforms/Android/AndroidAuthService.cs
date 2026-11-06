using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Firebase.Auth;

namespace CompraPublicCercaMobile.Services;

public sealed class AndroidAuthService : IAuthService
{
    private const int GoogleSignInRequestCode = 9001;

    private static TaskCompletionSource<Intent?>? _pendingSignIn;

    public async Task<AuthUser?> SignInWithGoogleAsync()
    {
        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("No hay una actividad activa.");

        var options = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
            .RequestIdToken(GetWebClientId(activity))
            .RequestEmail()
            .Build();
        var client = GoogleSignIn.GetClient(activity, options);

        // Always show the account picker instead of reusing the last account.
        await client.SignOutAsync();

        _pendingSignIn = new TaskCompletionSource<Intent?>();
        activity.StartActivityForResult(client.SignInIntent, GoogleSignInRequestCode);
        var data = await _pendingSignIn.Task;

        GoogleSignInAccount account;
        try
        {
            account = await GoogleSignIn.GetSignedInAccountFromIntent(data).AsAsync<GoogleSignInAccount>();
        }
        catch (ApiException ex) when (ex.StatusCode == GoogleSignInStatusCodes.SignInCancelled)
        {
            return null;
        }

        var credential = GoogleAuthProvider.GetCredential(account.IdToken, null);
        var authResult = await FirebaseAuth.Instance.SignInWithCredentialAsync(credential);
        var user = authResult.User
            ?? throw new InvalidOperationException("Firebase no devolvió un usuario.");

        return new AuthUser(user.Uid, user.DisplayName, user.Email);
    }

    public static void HandleActivityResult(int requestCode, Intent? data)
    {
        if (requestCode == GoogleSignInRequestCode)
        {
            _pendingSignIn?.TrySetResult(data);
        }
    }

    // default_web_client_id is generated at build time from google-services.json.
    private static string GetWebClientId(Activity activity)
    {
        var resourceId = activity.Resources!.GetIdentifier("default_web_client_id", "string", activity.PackageName);
        if (resourceId == 0)
        {
            throw new InvalidOperationException("Falta Platforms/Android/google-services.json. Revisa docs/firebase-auth.md.");
        }

        return activity.GetString(resourceId);
    }
}
