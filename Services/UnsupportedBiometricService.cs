using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

/// <summary>Plataformas sin biometría implementada (Windows, Mac Catalyst): la opción no se muestra.</summary>
public sealed class UnsupportedBiometricService : IBiometricService
{
    public BiometricKind GetAvailableKind() => BiometricKind.None;

    public Task<BiometricResult> AuthenticateAsync(string reason) => Task.FromResult(BiometricResult.Unavailable);
}
