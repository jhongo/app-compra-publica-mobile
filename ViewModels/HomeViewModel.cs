using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private const string DefaultUserName = "proveedor";

    private readonly IProcessService _processService;
    private readonly ISessionService _session;
    private readonly IUserLocationService _userLocation;
    private readonly IFavoritesService _favorites;
    private readonly INavigationService _navigation;
    private readonly ILogger<HomeViewModel> _logger;

    private CancellationTokenSource? _loadCts;
    private bool _isInitialized;
    private int _currentPage;

    [ObservableProperty]
    private string _greeting = string.Empty;

    [ObservableProperty]
    private string _userInitial = "P";

    /// <summary>Provincia elegida por el usuario, o vacío si aún no eligió.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLocation), nameof(LocationText), nameof(NearbyHint))]
    private string _location = string.Empty;

    public bool HasLocation => Location.Length > 0;

    public string LocationText => HasLocation ? Location : "Elige tu provincia";

    public string NearbyHint => HasLocation
        ? $"Primero las de {Location} · fuente: SERCOP"
        : "Elige tu provincia para ver primero las cercanas · fuente: SERCOP";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchText = string.Empty;

    /// <summary>Palabra clave de los resultados que se están mostrando.</summary>
    [ObservableProperty]
    private string _activeKeyword = string.Empty;

    [ObservableProperty]
    private int _totalResults;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool _hasMorePages;

    /// <summary>La búsqueda terminó bien pero sin resultados.</summary>
    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>Hay resultados para mostrar (resumen y lista).</summary>
    [ObservableProperty]
    private bool _hasResults;

    public int Year { get; } = DateTime.Today.Year;

    public ObservableCollection<CategoryItemViewModel> Categories { get; } =
    [
        new("Mantenimiento", "mantenimiento"),
        new("Construcción", "construcción"),
        new("Limpieza", "limpieza"),
        new("Seguridad", "seguridad"),
        new("Vehículos", "vehiculos"),
        new("Alimentos", "alimentos"),
        new("Medicamentos", "medicamentos")
    ];

    public ObservableCollection<OpportunityItemViewModel> Opportunities { get; } = new();

    public HomeViewModel(
        IProcessService processService,
        ISessionService session,
        IUserLocationService userLocation,
        IFavoritesService favorites,
        INavigationService navigation,
        ILogger<HomeViewModel> logger)
    {
        _favorites = favorites;
        _processService = processService;
        _session = session;
        _userLocation = userLocation;
        _navigation = navigation;
        _logger = logger;
        Title = "Inicio";
    }

    /// <summary>Se llama cada vez que aparece la pantalla; solo carga datos la primera vez.</summary>
    public async Task InitializeAsync()
    {
        LoadGreeting();
        RefreshLocation();
        RefreshFavorites();

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        var first = Categories[0];
        first.IsSelected = true;
        await LoadAsync(first.Keyword);
    }

    [RelayCommand]
    private Task SelectCategoryAsync(CategoryItemViewModel category)
    {
        SelectOnly(category);
        SearchText = string.Empty;
        return LoadAsync(category.Keyword);
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task SearchAsync()
    {
        SelectOnly(null);
        return LoadAsync(SearchText.Trim());
    }

    private bool CanSearch() => SearchText.Trim().Length >= ApiConstants.MinSearchLength;

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync(ActiveKeyword, isRefresh: true);

    [RelayCommand]
    private Task RetryAsync() => LoadAsync(ActiveKeyword);

    [RelayCommand(CanExecute = nameof(HasMorePages))]
    private async Task LoadMoreAsync()
    {
        if (IsLoadingMore || IsBusy)
        {
            return;
        }

        var token = _loadCts?.Token ?? CancellationToken.None;
        IsLoadingMore = true;
        try
        {
            var result = await _processService.SearchAsync(
                new ProcessSearchQuery(ActiveKeyword, Year) { Page = _currentPage + 1 }, token);
            foreach (var item in ToItems(result.Items))
            {
                Opportunities.Add(item);
            }

            _currentPage = result.Page;
            HasMorePages = result.HasMorePages;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Se inició otra búsqueda: se descarta esta página.
        }
        catch (Exception ex)
        {
            // Se mantienen los resultados ya cargados; el usuario puede volver a intentarlo.
            _logger.LogWarning(ex, "No se pudo cargar la página {Page} de «{Keyword}».", _currentPage + 1, ActiveKeyword);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    private Task ChangeLocationAsync() => _navigation.GoToAsync(NavigationRoutes.LocationPicker);

    [RelayCommand]
    private Task OpenDetailAsync(OpportunityItemViewModel item) =>
        _navigation.GoToAsync(
            NavigationRoutes.ProcessDetail,
            new Dictionary<string, object> { [NavigationRoutes.OcidParameter] = item.Opportunity.Ocid });

    [RelayCommand]
    private void ToggleFavorite(OpportunityItemViewModel item) =>
        item.IsFavorite = _favorites.Toggle(item.Opportunity);

    private async Task LoadAsync(string keyword, bool isRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            IsRefreshing = false;
            return;
        }

        // Cancela la búsqueda anterior si el usuario cambia de categoría rápido.
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ActiveKeyword = keyword;
        ErrorMessage = string.Empty;
        IsEmpty = false;

        if (!isRefresh)
        {
            IsBusy = true;
            HasResults = false;
            Opportunities.Clear();
        }

        try
        {
            var result = await _processService.SearchAsync(new ProcessSearchQuery(keyword, Year), token);

            Opportunities.Clear();
            foreach (var item in ToItems(result.Items).OrderByDescending(i => i.Opportunity.IsNearby))
            {
                Opportunities.Add(item);
            }

            TotalResults = result.Total;
            _currentPage = result.Page;
            HasMorePages = result.HasMorePages;
            HasResults = Opportunities.Count > 0;
            IsEmpty = !HasResults;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Lo reemplazó una búsqueda más reciente.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la búsqueda de procesos «{Keyword}».", keyword);
            Opportunities.Clear();
            HasResults = false;
            HasMorePages = false;
            ErrorMessage = "No pudimos consultar los procesos de SERCOP. Revisa tu conexión e inténtalo de nuevo.";
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }
    }

    private IEnumerable<OpportunityItemViewModel> ToItems(IEnumerable<Opportunity> items) =>
        items.Select(o => new OpportunityItemViewModel(
            o with { IsNearby = _userLocation.IsUserProvince(o.Region) },
            _favorites.IsFavorite(o.Ocid)));

    /// <summary>Un favorito pudo quitarse o agregarse en otra pantalla (Favoritos, detalle).</summary>
    private void RefreshFavorites()
    {
        foreach (var item in Opportunities)
        {
            item.IsFavorite = _favorites.IsFavorite(item.Opportunity.Ocid);
        }
    }

    /// <summary>
    /// Lee la provincia guardada. Si cambió (p. ej. al volver de elegir otra), vuelve a
    /// marcar y ordenar los resultados ya cargados, sin consultar de nuevo la API.
    /// </summary>
    private void RefreshLocation()
    {
        var province = _userLocation.Province ?? string.Empty;
        if (province == Location)
        {
            return;
        }

        Location = province;
        var reordered = ToItems(Opportunities.Select(i => i.Opportunity))
            .OrderByDescending(i => i.Opportunity.IsNearby)
            .ToList();
        Opportunities.Clear();
        foreach (var item in reordered)
        {
            Opportunities.Add(item);
        }
    }

    private void SelectOnly(CategoryItemViewModel? selected)
    {
        foreach (var category in Categories)
        {
            category.IsSelected = category == selected;
        }
    }

    private void LoadGreeting()
    {
        var firstName = _session.CurrentUser?.DisplayName?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        var name = firstName ?? DefaultUserName;
        Greeting = $"Hola, {name}";
        UserInitial = name[..1].ToUpperInvariant();
    }
}
