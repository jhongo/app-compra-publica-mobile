using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.ViewModels;

public partial class FavoritesViewModel : BaseViewModel
{
    public ObservableCollection<Opportunity> Favorites { get; } = new();

    public FavoritesViewModel()
    {
        Title = "Favoritos";
        LoadSampleFavorites();
    }

    // Solo quita el elemento de la lista de ejemplo; no se guarda nada.
    [RelayCommand]
    private void RemoveFavorite(Opportunity? favorite)
    {
        if (favorite is not null)
        {
            Favorites.Remove(favorite);
        }
    }

    // TODO: reemplazar por los favoritos guardados del usuario cuando exista el servicio.
    private void LoadSampleFavorites()
    {
        Favorites.Add(new Opportunity(
            "demo-1",
            "Mantenimiento de equipos informáticos",
            86,
            new DateTime(2026, 12, 5)));
        Favorites.Add(new Opportunity(
            "demo-2",
            "Adquisición de materiales de oficina",
            72,
            new DateTime(2026, 12, 10)));
        Favorites.Add(new Opportunity(
            "demo-3",
            "Servicio de limpieza para edificio institucional",
            64,
            new DateTime(2026, 12, 18)));
    }
}
