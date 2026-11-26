using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Procesos que el usuario marcó como favoritos. Se guardan en el dispositivo
/// para poder verlos sin volver a consultar SERCOP.
/// </summary>
public interface IFavoritesService
{
    /// <summary>Favoritos guardados, del más reciente al más antiguo.</summary>
    IReadOnlyList<Opportunity> GetAll();

    int Count { get; }

    bool IsFavorite(string ocid);

    /// <summary>Agrega el proceso si no estaba o lo quita si ya estaba. Devuelve el nuevo estado.</summary>
    bool Toggle(Opportunity opportunity);

    void Remove(string ocid);
}
