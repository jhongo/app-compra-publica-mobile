namespace CompraPublicCercaMobile.Models;

/// <summary>Datos del formulario de registro manual.</summary>
public sealed record Registration(
    string FirstNames,
    string LastNames,
    string Email,
    string Password,
    string PasswordConfirmation,
    string VerificationToken,
    string Province,
    string? Address,
    string? PostalCode,
    DateTime? BirthDate);
