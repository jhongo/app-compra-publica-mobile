using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CompraPublicCercaMobile.Constants;

/// <summary>
/// Validaciones rápidas en el teléfono, para avisar antes de llamar a la API.
/// Son las mismas reglas que aplica la API (la API siempre tiene la última palabra).
/// </summary>
public static partial class InputRules
{
    public const int MinPasswordLength = 8;

    public const string PasswordHint = "Mínimo 8 caracteres, con letras y números.";

    public static bool IsValidEmail(string? email)
    {
        var value = email?.Trim() ?? string.Empty;
        return MailAddress.TryCreate(value, out var address)
            && address.Address == value
            && address.Host.Contains('.');
    }

    public static string? PasswordError(string? password) => password switch
    {
        null or "" => "Escribe una contraseña.",
        { Length: < MinPasswordLength } => $"La contraseña debe tener al menos {MinPasswordLength} caracteres.",
        _ when !password.Any(char.IsLetter) => "La contraseña debe tener al menos una letra.",
        _ when !password.Any(char.IsDigit) => "La contraseña debe tener al menos un número.",
        _ => null
    };

    public static bool IsValidPostalCode(string? value) =>
        string.IsNullOrWhiteSpace(value) || SixDigits().IsMatch(value.Trim());

    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex SixDigits();
}
