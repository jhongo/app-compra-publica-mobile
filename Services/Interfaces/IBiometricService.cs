namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Face ID / Touch ID / huella del teléfono. Solo confirma que quien usa el teléfono es
/// su dueño: la cara o la huella nunca salen del dispositivo ni llegan a la API.
/// </summary>
public interface IBiometricService
{
    /// <summary>Tipo disponible y configurado en el teléfono; None si no hay o no está activado.</summary>
    BiometricKind GetAvailableKind();

    Task<BiometricResult> AuthenticateAsync(string reason);
}

public enum BiometricKind
{
    None,
    FaceId,
    TouchId,
    Fingerprint,
    Generic
}

public enum BiometricResult
{
    Success,
    /// <summary>El usuario canceló o eligió usar la contraseña.</summary>
    Cancelled,
    /// <summary>No hay biometría configurada o está bloqueada por demasiados intentos.</summary>
    Unavailable,
    Failed
}

public static class BiometricKindExtensions
{
    /// <summary>Nombre para mostrar en pantalla: "Face ID", "huella"...</summary>
    public static string DisplayName(this BiometricKind kind) => kind switch
    {
        BiometricKind.FaceId => "Face ID",
        BiometricKind.TouchId => "Touch ID",
        BiometricKind.Fingerprint => "huella",
        _ => "biometría"
    };
}
