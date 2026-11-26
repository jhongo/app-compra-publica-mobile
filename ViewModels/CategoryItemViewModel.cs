using CommunityToolkit.Mvvm.ComponentModel;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>Chip de categoría rápida en Inicio.</summary>
public partial class CategoryItemViewModel : ObservableObject
{
    public CategoryItemViewModel(string name, string keyword)
    {
        Name = name;
        Keyword = keyword;
    }

    public string Name { get; }

    /// <summary>Palabra clave que se envía a la API (las tildes cambian los resultados).</summary>
    public string Keyword { get; }

    [ObservableProperty]
    private bool _isSelected;
}
