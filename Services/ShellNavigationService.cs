using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        var shell = Shell.Current
            ?? throw new InvalidOperationException("La app no está usando AppShell.");

        return parameters is null
            ? shell.GoToAsync(route)
            : shell.GoToAsync(route, new ShellNavigationQueryParameters(parameters));
    }
}
