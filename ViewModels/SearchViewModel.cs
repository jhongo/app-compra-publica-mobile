using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>
/// Búsqueda avanzada de procesos. Año, palabra clave, entidad, proveedor y tipo de
/// procedimiento los filtra la API; provincia y monto los filtra la app (LocalProcessFilter).
/// </summary>
public partial class SearchViewModel : BaseViewModel
{
    public const string AllProvinces = "Todas las provincias";

    private const int ResultsPerBatch = 10;
    private const int MaxPagesPerBatch = 5;

    private readonly IProcessService _processService;
    private readonly IUserLocationService _userLocation;
    private readonly IFavoritesService _favorites;
    private readonly INavigationService _navigation;
    private readonly ILogger<SearchViewModel> _logger;

    private CancellationTokenSource? _searchCts;
    private ProcessSearchQuery? _lastQuery;
    private LocalProcessFilter _lastFilter = LocalProcessFilter.None;
    private int _lastPage;
    private int _scannedCount;

    // ===== Filtros que aplica la API =====

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private int _selectedYear = DateTime.Today.Year;

    [ObservableProperty]
    private string _buyer = string.Empty;

    [ObservableProperty]
    private string _supplier = string.Empty;

    // ===== Filtros que aplica la app =====

    [ObservableProperty]
    private string _selectedProvince = AllProvinces;

    // ===== Estado de la pantalla =====

    [ObservableProperty]
    private bool _isFiltersExpanded = true;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasSearched;

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool _hasMorePages;

    [ObservableProperty]
    private string _resultsSummary = string.Empty;

    /// <summary>Aviso cuando provincia o monto se filtran solo sobre lo revisado.</summary>
    [ObservableProperty]
    private string _localFilterNote = string.Empty;

    [ObservableProperty]
    private string _filtersSummary = string.Empty;

    [ObservableProperty]
    private string _methodDescription = string.Empty;

    public IReadOnlyList<int> Years { get; } = Enumerable
        .Range(ApiConstants.FirstAvailableYear, DateTime.Today.Year - ApiConstants.FirstAvailableYear + 1)
        .Reverse()
        .ToList();

    public IReadOnlyList<string> Provinces { get; } = [AllProvinces, .. EcuadorProvinces.All];

    public ObservableCollection<FilterOptionViewModel> MethodOptions { get; } =
    [
        new("Todos", ProcurementMethod.Any, "Incluye todos los tipos de procedimiento."),
        new("Abiertos", ProcurementMethod.Open, "Licitación, subasta inversa, cotización y similares: cualquier proveedor puede ofertar."),
        new("Selectivos", ProcurementMethod.Selective, "Catálogo electrónico con mejor oferta entre proveedores calificados."),
        new("Directos", ProcurementMethod.Direct, "Compra directa por catálogo, ínfima cuantía y similares.")
    ];

    public ObservableCollection<FilterOptionViewModel> AmountOptions { get; } =
    [
        new("Cualquier monto", AmountRange.Any),
        new("Hasta $5.000", AmountRange.UpTo5K),
        new("$5.001 – $10.000", AmountRange.From5KTo10K),
        new("$10.001 – $50.000", AmountRange.From10KTo50K),
        new("Más de $50.000", AmountRange.Over50K)
    ];

    /// <summary>Sugerencias de palabras clave (la API no tiene filtro de categoría).</summary>
    public IReadOnlyList<string> KeywordSuggestions { get; } =
        ["mantenimiento", "construcción", "limpieza", "seguridad", "alimentos", "medicamentos", "computadoras", "consultoría"];

    public ObservableCollection<OpportunityItemViewModel> Results { get; } = new();

    public SearchViewModel(
        IProcessService processService,
        IUserLocationService userLocation,
        IFavoritesService favorites,
        INavigationService navigation,
        ILogger<SearchViewModel> logger)
    {
        _favorites = favorites;
        _processService = processService;
        _userLocation = userLocation;
        _navigation = navigation;
        _logger = logger;
        Title = "Buscar";

        MethodOptions[0].IsSelected = true;
        AmountOptions[0].IsSelected = true;
        MethodDescription = MethodOptions[0].Description;
        UpdateFiltersSummary();
    }

    public bool HasUserProvince => _userLocation.HasProvince;

    public string UseMyProvinceText => _userLocation.Province is { } p ? $"Usar {p}" : string.Empty;

    /// <summary>Se llama al mostrar la pantalla: la provincia del usuario puede haber cambiado.</summary>
    public void OnAppearing()
    {
        OnPropertyChanged(nameof(HasUserProvince));
        OnPropertyChanged(nameof(UseMyProvinceText));

        // Un favorito pudo quitarse o agregarse en otra pantalla (Favoritos, detalle).
        foreach (var item in Results)
        {
            item.IsFavorite = _favorites.IsFavorite(item.Opportunity.Ocid);
        }
    }

    partial void OnSelectedYearChanged(int value) => UpdateFiltersSummary();

    partial void OnSelectedProvinceChanged(string value) => UpdateFiltersSummary();

    partial void OnBuyerChanged(string value) => UpdateFiltersSummary();

    partial void OnSupplierChanged(string value) => UpdateFiltersSummary();

    [RelayCommand]
    private void SelectMethod(FilterOptionViewModel option)
    {
        SelectOnly(MethodOptions, option);
        MethodDescription = option.Description;
        UpdateFiltersSummary();
    }

    [RelayCommand]
    private void SelectAmount(FilterOptionViewModel option)
    {
        SelectOnly(AmountOptions, option);
        UpdateFiltersSummary();
    }

    [RelayCommand]
    private Task UseSuggestionAsync(string keyword)
    {
        Keyword = keyword;
        return SearchAsync();
    }

    [RelayCommand]
    private void UseMyProvince()
    {
        if (_userLocation.Province is { } province)
        {
            SelectedProvince = province;
        }
    }

    [RelayCommand]
    private void ToggleFilters() => IsFiltersExpanded = !IsFiltersExpanded;

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedYear = DateTime.Today.Year;
        Buyer = string.Empty;
        Supplier = string.Empty;
        SelectedProvince = AllProvinces;
        SelectMethod(MethodOptions[0]);
        SelectAmount(AmountOptions[0]);
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (!TryBuildQuery(out var query, out var filter))
        {
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        IsBusy = true;
        HasSearched = true;
        HasResults = false;
        IsEmpty = false;
        HasMorePages = false;
        ErrorMessage = string.Empty;
        Results.Clear();
        _scannedCount = 0;

        try
        {
            var result = await FetchAsync(query, filter, token);
            AddResults(result);
            _lastQuery = query;
            _lastFilter = filter;
            IsFiltersExpanded = false;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Lo reemplazó una búsqueda más reciente.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la búsqueda avanzada «{Keyword}».", query.Keyword);
            ErrorMessage = "No pudimos consultar los procesos de SERCOP. Revisa tu conexión e inténtalo de nuevo.";
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasMorePages))]
    private async Task LoadMoreAsync()
    {
        if (_lastQuery is null || IsLoadingMore || IsBusy)
        {
            return;
        }

        var token = _searchCts?.Token ?? CancellationToken.None;
        IsLoadingMore = true;
        try
        {
            var result = await FetchAsync(_lastQuery with { Page = _lastPage + 1 }, _lastFilter, token);
            AddResults(result);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron cargar más resultados.");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    private Task OpenDetailAsync(OpportunityItemViewModel item) =>
        _navigation.GoToAsync(
            NavigationRoutes.ProcessDetail,
            new Dictionary<string, object> { [NavigationRoutes.OcidParameter] = item.Opportunity.Ocid });

    [RelayCommand]
    private void ToggleFavorite(OpportunityItemViewModel item) =>
        item.IsFavorite = _favorites.Toggle(item.Opportunity);

    private async Task<FilteredSearchResult> FetchAsync(
        ProcessSearchQuery query, LocalProcessFilter filter, CancellationToken token)
    {
        if (filter.IsActive)
        {
            return await _processService.SearchFilteredAsync(
                query, filter, ResultsPerBatch, MaxPagesPerBatch, token);
        }

        var page = await _processService.SearchAsync(query, token);
        return new FilteredSearchResult(page.Items, page.Total, page.Page, page.Pages, page.Items.Count);
    }

    private void AddResults(FilteredSearchResult result)
    {
        foreach (var item in result.Items)
        {
            Results.Add(new OpportunityItemViewModel(
                item with { IsNearby = _userLocation.IsUserProvince(item.Region) },
                _favorites.IsFavorite(item.Ocid)));
        }

        _lastPage = result.LastPage;
        _scannedCount += result.ScannedCount;
        HasMorePages = result.HasMorePages;
        HasResults = Results.Count > 0;
        IsEmpty = !HasResults;

        ResultsSummary = result.Total == 1
            ? "1 proceso encontrado en SERCOP"
            : $"{result.Total.ToString("N0", DisplayFormat.Culture)} procesos encontrados en SERCOP";

        LocalFilterNote = _lastFilter.IsActive
            ?$"{Results.Count} cumplen provincia o monto entre los {_scannedCount.ToString("N0", DisplayFormat.Culture)} revisados. " +
              "Estos dos filtros los aplica la app, no SERCOP."
            : string.Empty;
    }

    private bool TryBuildQuery(out ProcessSearchQuery query, out LocalProcessFilter filter)
    {
        var keyword = Keyword.Trim();
        var buyer = Buyer.Trim();
        var supplier = Supplier.Trim();
        var min = ApiConstants.MinSearchLength;

        ValidationMessage = keyword.Length < min ? $"Escribe al menos {min} letras en la palabra clave."
            : buyer.Length is > 0 and < 3 ? $"La entidad necesita al menos {min} letras."
            : supplier.Length is > 0 and < 3 ? $"El proveedor necesita al menos {min} letras."
            : string.Empty;

        query = new ProcessSearchQuery(keyword, SelectedYear)
        {
            Buyer = buyer.Length > 0 ? buyer : null,
            Supplier = supplier.Length > 0 ? supplier : null,
            Method = (ProcurementMethod)Selected(MethodOptions).Value
        };
        filter = new LocalProcessFilter
        {
            Province = SelectedProvince == AllProvinces ? null : SelectedProvince,
            AmountRange = (AmountRange)Selected(AmountOptions).Value
        };

        if (ValidationMessage.Length > 0)
        {
            IsFiltersExpanded = true;
            return false;
        }

        // El filtro local se usa en AddResults para el aviso.
        _lastFilter = filter;
        return true;
    }

    private void UpdateFiltersSummary()
    {
        var parts = new List<string> { SelectedYear.ToString() };
        if (Selected(MethodOptions).Value is ProcurementMethod method && method != ProcurementMethod.Any)
        {
            parts.Add(Selected(MethodOptions).Label);
        }

        if (SelectedProvince != AllProvinces)
        {
            parts.Add(SelectedProvince);
        }

        if (Selected(AmountOptions).Value is AmountRange range && range != AmountRange.Any)
        {
            parts.Add(Selected(AmountOptions).Label);
        }

        if (!string.IsNullOrWhiteSpace(Buyer))
        {
            parts.Add($"Entidad: {Buyer.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(Supplier))
        {
            parts.Add($"Proveedor: {Supplier.Trim()}");
        }

        FiltersSummary = string.Join(" · ", parts);
    }

    private static FilterOptionViewModel Selected(IEnumerable<FilterOptionViewModel> options) =>
        options.FirstOrDefault(o => o.IsSelected) ?? options.First();

    private static void SelectOnly(IEnumerable<FilterOptionViewModel> options, FilterOptionViewModel selected)
    {
        foreach (var option in options)
        {
            option.IsSelected = option == selected;
        }
    }
}
