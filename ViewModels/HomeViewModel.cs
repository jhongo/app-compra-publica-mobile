using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private const string DefaultUserName = "proveedor";
    private const string SearchQueryParameter = "query";

    private readonly ISessionService _session;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string _greeting = string.Empty;

    // TODO: obtener la provincia con el servicio de ubicación cuando exista.
    [ObservableProperty]
    private string _location = "Pichincha";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _suggestedSearch = "mantenimiento informático";

    public ObservableCollection<Opportunity> RecommendedOpportunities { get; } = new();

    public HomeViewModel(ISessionService session, INavigationService navigation)
    {
        _session = session;
        _navigation = navigation;
        Title = "Inicio";

        LoadGreeting();
        LoadRecommendedOpportunities();
    }

    [RelayCommand]
    private void UseSuggestedSearch() => SearchText = SuggestedSearch;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task SearchAsync() =>
        _navigation.GoToAsync(
            $"//{NavigationRoutes.Buscar}",
            new Dictionary<string, object> { [SearchQueryParameter] = SearchText.Trim() });

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchText);

    private void LoadGreeting()
    {
        var firstName = _session.CurrentUser?.DisplayName?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        Greeting = $"Hola, {firstName ?? DefaultUserName}";
    }

    // TODO: reemplazar por la consulta a la API externa de compras públicas.
    private void LoadRecommendedOpportunities()
    {
        RecommendedOpportunities.Add(new Opportunity(
            "demo-1",
            "Mantenimiento de equipos informáticos",
            86,
            new DateTime(2026, 11, 20)));
    }
}
