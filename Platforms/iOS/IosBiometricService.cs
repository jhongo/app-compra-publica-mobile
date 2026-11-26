using CompraPublicCercaMobile.Services.Interfaces;
using LocalAuthentication;

namespace CompraPublicCercaMobile.Services;

/// <summary>Face ID / Touch ID con LocalAuthentication. Requiere NSFaceIDUsageDescription en Info.plist.</summary>
public sealed class IosBiometricService : IBiometricService
{
    public BiometricKind GetAvailableKind()
    {
        using var context = new LAContext();
        if (!context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _))
        {
            return BiometricKind.None;
        }

        return context.BiometryType switch
        {
            LABiometryType.FaceId => BiometricKind.FaceId,
            LABiometryType.TouchId => BiometricKind.TouchId,
            _ => BiometricKind.Generic
        };
    }

    public async Task<BiometricResult> AuthenticateAsync(string reason)
    {
        using var context = new LAContext
        {
            // Botón de respaldo del diálogo del sistema: vuelve al login con contraseña.
            LocalizedFallbackTitle = "Usar contraseña",
            LocalizedCancelTitle = "Cancelar"
        };

        var result = await context.EvaluatePolicyAsync(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, reason);
        if (result.Item1)
        {
            return BiometricResult.Success;
        }

        return (LAStatus)(long)(result.Item2?.Code ?? 0) switch
        {
            LAStatus.UserCancel or LAStatus.AppCancel or LAStatus.SystemCancel or LAStatus.UserFallback
                => BiometricResult.Cancelled,
            LAStatus.BiometryNotAvailable or LAStatus.BiometryNotEnrolled or LAStatus.BiometryLockout
                => BiometricResult.Unavailable,
            _ => BiometricResult.Failed
        };
    }
}
