using Android.Content;
using Android.Content.PM;
using Android.Hardware.Biometrics;
using System.Runtime.Versioning;
using Android.OS;
using CompraPublicCercaMobile.Services.Interfaces;
using Java.Lang;
using CancellationSignal = Android.OS.CancellationSignal;

namespace CompraPublicCercaMobile.Services;

/// <summary>
/// Huella / rostro con BiometricPrompt del sistema (Android 10+, API 29). No necesita
/// paquetes extra. En versiones anteriores la opción simplemente no se ofrece.
/// Requiere el permiso USE_BIOMETRIC en AndroidManifest.xml.
/// </summary>
public sealed class AndroidBiometricService : IBiometricService
{
    public BiometricKind GetAvailableKind()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(29)
            || Platform.AppContext.GetSystemService(Context.BiometricService) is not BiometricManager manager)
        {
            return BiometricKind.None;
        }

        var status = OperatingSystem.IsAndroidVersionAtLeast(30)
            ? manager.CanAuthenticate((int)BiometricManagerAuthenticators.BiometricStrong)
            : manager.CanAuthenticate();
        if (status != BiometricCode.Success)
        {
            return BiometricKind.None;
        }

        return Platform.AppContext.PackageManager?.HasSystemFeature(PackageManager.FeatureFingerprint) == true
            ? BiometricKind.Fingerprint
            : BiometricKind.Generic;
    }

    public Task<BiometricResult> AuthenticateAsync(string reason)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(29) || Platform.CurrentActivity is not { } activity)
        {
            return Task.FromResult(BiometricResult.Unavailable);
        }

        var completion = new TaskCompletionSource<BiometricResult>();
        var executor = activity.MainExecutor!;

        var prompt = new BiometricPrompt.Builder(activity)
            .SetTitle("Compra Pública Cerca")
            .SetSubtitle(reason)
            .SetNegativeButton("Usar contraseña", executor, new NegativeButtonListener(completion))
            .Build();

        prompt.Authenticate(new CancellationSignal(), executor, new Callback(completion));
        return completion.Task;
    }

    // Solo se crea dentro de AuthenticateAsync, que ya exige Android 10 (API 29).
    [SupportedOSPlatform("android29.0")]
    private sealed class Callback(TaskCompletionSource<BiometricResult> completion) : BiometricPrompt.AuthenticationCallback
    {
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? result) =>
            completion.TrySetResult(BiometricResult.Success);

        public override void OnAuthenticationError(BiometricErrorCode errorCode, ICharSequence? errString) =>
            completion.TrySetResult(errorCode switch
            {
                BiometricErrorCode.UserCanceled or BiometricErrorCode.Canceled => BiometricResult.Cancelled,
                BiometricErrorCode.Lockout or BiometricErrorCode.LockoutPermanent
                    or BiometricErrorCode.NoBiometrics or BiometricErrorCode.HwUnavailable => BiometricResult.Unavailable,
                _ => BiometricResult.Failed
            });

        // Un intento fallido (dedo no reconocido): el diálogo sigue abierto para reintentar.
        public override void OnAuthenticationFailed()
        {
        }
    }

    private sealed class NegativeButtonListener(TaskCompletionSource<BiometricResult> completion)
        : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        public void OnClick(IDialogInterface? dialog, int which) => completion.TrySetResult(BiometricResult.Cancelled);
    }
}
