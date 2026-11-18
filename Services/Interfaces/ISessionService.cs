using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Guarda al usuario autenticado mientras la app está abierta.
/// Se registra como Singleton para que todas las pantallas vean la misma sesión.
/// </summary>
public interface ISessionService
{
    AuthUser? CurrentUser { get; }

    bool IsAuthenticated { get; }

    void SetUser(AuthUser user);

    void Clear();
}
