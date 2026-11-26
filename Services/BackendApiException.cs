using System.Net;

namespace CompraPublicCercaMobile.Services;

/// <summary>
/// Error de la API propia, con un mensaje ya apto para mostrar al usuario.
/// En validaciones (422) trae además los errores por campo: "email", "password", "code"...
/// </summary>
public sealed class BackendApiException : Exception
{
    public BackendApiException(HttpStatusCode? statusCode, string message, IReadOnlyDictionary<string, string>? fieldErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }

    /// <summary>Null si no hubo respuesta (sin conexión, servidor apagado, tiempo agotado).</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Primer error de cada campo, con las claves que usa la API (snake_case).</summary>
    public IReadOnlyDictionary<string, string> FieldErrors { get; }

    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;

    public string? ErrorFor(string field) => FieldErrors.TryGetValue(field, out var error) ? error : null;
}
