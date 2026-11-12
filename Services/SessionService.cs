using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

public sealed class SessionService : ISessionService
{
    public AuthUser? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public void SetUser(AuthUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        CurrentUser = user;
    }

    public void Clear() => CurrentUser = null;
}
