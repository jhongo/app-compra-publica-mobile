namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Navegación entre pantallas sin que los ViewModels dependan de Shell,
/// así se pueden probar con una implementación falsa.
/// </summary>
public interface INavigationService
{
    /// <param name="route">Una ruta de <see cref="Constants.NavigationRoutes"/>; usa "//" para rutas raíz.</param>
    /// <param name="parameters">Identificadores para la pantalla destino (IQueryAttributable).</param>
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
}
