namespace CompraPublicCercaMobile.Models;

/// <summary>Usuario con sesión iniciada, tal como lo devuelve la API (GET /me).</summary>
public sealed record AuthUser(
    long Id,
    string FirstNames,
    string LastNames,
    string Email,
    string? Province,
    bool HasPassword,
    bool GoogleLinked)
{
    public string DisplayName => $"{FirstNames} {LastNames}".Trim();
}
