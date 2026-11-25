
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CompraPublicCercaMobile.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedLocation = string.Empty;

    [ObservableProperty]
    private string selectedProcessType = string.Empty;

    [ObservableProperty]
    private string selectedAmount = string.Empty;

    public ObservableCollection<SearchResultItem> Results { get; } = new()
    {
        new SearchResultItem
        {
            Title = "Adquisición de materiales de oficina",
            Entity = "Institución pública (ejemplo)",
            Amount = "$2.500,00",
            Deadline = new DateTime(2026, 12, 10)
        },
        new SearchResultItem
        {
            Title = "Servicio de mantenimiento de equipos",
            Entity = "Entidad contratante (ejemplo)",
            Amount = "$5.800,00",
            Deadline = new DateTime(2026, 12, 15)
        }
    };

    public bool HasNoResults => Results.Count == 0;
}

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
}
