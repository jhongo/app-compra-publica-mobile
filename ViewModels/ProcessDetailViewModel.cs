using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>Detalle de un proceso. Recibe el OCID por navegación (docs/arquitectura.md, decisión 3).</summary>
public partial class ProcessDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IProcessService _processService;
    private readonly IFavoritesService _favorites;
    private readonly INavigationService _navigation;
    private readonly ILogger<ProcessDetailViewModel> _logger;

    private string _ocid = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetail))]
    private ProcessDetail? _detail;

    public bool HasDetail => Detail is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteDescription))]
    private bool _isFavorite;

    public string FavoriteDescription => IsFavorite ? "Quitar de favoritos" : "Agregar a favoritos";

    public ProcessDetailViewModel(
        IProcessService processService,
        IFavoritesService favorites,
        INavigationService navigation,
        ILogger<ProcessDetailViewModel> logger)
    {
        _processService = processService;
        _favorites = favorites;
        _navigation = navigation;
        _logger = logger;
        Title = "Detalle del proceso";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavigationRoutes.OcidParameter, out var value)
            && value?.ToString() is { Length: > 0 } ocid)
        {
            _ocid = ocid;
            IsFavorite = _favorites.IsFavorite(ocid);
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(_ocid))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            Detail = await _processService.GetDetailAsync(_ocid);
            if (Detail is null)
            {
                ErrorMessage = "No encontramos la información de este proceso.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la carga del proceso {Ocid}.", _ocid);
            ErrorMessage = "No pudimos cargar el detalle. Revisa tu conexión e inténtalo de nuevo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Solo se puede marcar cuando el detalle ya cargó: de ahí salen los datos que se guardan.</summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Detail is not null)
        {
            IsFavorite = _favorites.Toggle(Detail.ToOpportunity());
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigation.GoToAsync("..");
}
