using System.Text.Json;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Models.Storage;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.Services;

/// <summary>
/// Guarda los favoritos en un archivo JSON dentro de la carpeta privada de la app.
/// La lista es pequeña, así que se mantiene en memoria y se reescribe completa al cambiar.
/// </summary>
public sealed class FavoritesService : IFavoritesService
{
    private const string FileName = "favorites.json";

    private readonly string _filePath;
    private readonly ILogger<FavoritesService> _logger;
    private readonly List<Opportunity> _items;

    public FavoritesService(IFileSystem fileSystem, ILogger<FavoritesService> logger)
    {
        _filePath = Path.Combine(fileSystem.AppDataDirectory, FileName);
        _logger = logger;
        _items = Load();
    }

    public int Count => _items.Count;

    public IReadOnlyList<Opportunity> GetAll() => _items.ToList();

    public bool IsFavorite(string ocid) => _items.Exists(o => o.Ocid == ocid);

    public bool Toggle(Opportunity opportunity)
    {
        if (IsFavorite(opportunity.Ocid))
        {
            Remove(opportunity.Ocid);
            return false;
        }

        // "Cerca de ti" depende de la provincia actual del usuario: se recalcula al mostrar.
        _items.Insert(0, opportunity with { IsNearby = false });
        Save();
        return true;
    }

    public void Remove(string ocid)
    {
        if (_items.RemoveAll(o => o.Ocid == ocid) > 0)
        {
            Save();
        }
    }

    private List<Opportunity> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            using var stream = File.OpenRead(_filePath);
            return JsonSerializer.Deserialize(stream, FavoritesJsonContext.Default.ListOpportunity) ?? [];
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Un archivo dañado no debe impedir abrir la app: se empieza con la lista vacía.
            _logger.LogWarning(ex, "No se pudo leer {File}; se ignoran los favoritos guardados.", _filePath);
            return [];
        }
    }

    private void Save()
    {
        try
        {
            // Se escribe a un archivo temporal y luego se reemplaza, para no dejar
            // el archivo a medias si la app se cierra mientras guarda.
            var tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(_items, FavoritesJsonContext.Default.ListOpportunity));
            File.Move(tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "No se pudieron guardar los favoritos en {File}.", _filePath);
        }
    }
}
