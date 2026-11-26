using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>Procesos guardados como favoritos. Se leen del dispositivo, sin consultar SERCOP.</summary>
public partial class FavoritesViewModel : BaseViewModel
{
    private readonly IFavoritesService _favorites;
    private readonly IUserLocationService _userLocation;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(HasItems), nameof(CountText))]
    private int _count;

    public FavoritesViewModel(
        IFavoritesService favorites,
        IUserLocationService userLocation,
        INavigationService navigation)
    {
        _favorites = favorites;
        _userLocation = userLocation;
        _navigation = navigation;
        Title = "Favoritos";
    }

    public ObservableCollection<OpportunityItemViewModel> Items { get; } = new();

    public bool IsEmpty => Count == 0;

    public bool HasItems => Count > 0;

    public string CountText => Count == 1 ? "1 proceso guardado" : $"{Count} procesos guardados";

    /// <summary>Se llama cada vez que aparece la pestaña: los favoritos pudieron cambiar en otra pantalla.</summary>
    public void Load()
    {
        Items.Clear();
        foreach (var opportunity in _favorites.GetAll())
        {
            Items.Add(new OpportunityItemViewModel(
                opportunity with { IsNearby = _userLocation.IsUserProvince(opportunity.Region) },
                isFavorite: true));
        }

        Count = Items.Count;
    }

    /// <summary>En esta pantalla tocar la estrella quita el proceso de la lista.</summary>
    [RelayCommand]
    private void ToggleFavorite(OpportunityItemViewModel item)
    {
        _favorites.Remove(item.Opportunity.Ocid);
        Items.Remove(item);
        Count = Items.Count;
    }

    [RelayCommand]
    private Task OpenDetailAsync(OpportunityItemViewModel item) =>
        _navigation.GoToAsync(
            NavigationRoutes.ProcessDetail,
            new Dictionary<string, object> { [NavigationRoutes.OcidParameter] = item.Opportunity.Ocid });

    [RelayCommand]
    private Task GoToSearchAsync() => _navigation.GoToAsync($"//{NavigationRoutes.Buscar}");
}
