using CommunityToolkit.Mvvm.ComponentModel;
using CompraPublicCercaMobile.Models;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>
/// Proceso mostrado en una lista (Inicio, Buscar, Favoritos). Envuelve el modelo
/// inmutable y agrega el estado que cambia en pantalla: si es favorito.
/// </summary>
public partial class OpportunityItemViewModel : ObservableObject
{
    public OpportunityItemViewModel(Opportunity opportunity, bool isFavorite)
    {
        Opportunity = opportunity;
        _isFavorite = isFavorite;
    }

    public Opportunity Opportunity { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteDescription))]
    private bool _isFavorite;

    /// <summary>Texto para lectores de pantalla del botón de estrella.</summary>
    public string FavoriteDescription => IsFavorite ? "Quitar de favoritos" : "Agregar a favoritos";
}
