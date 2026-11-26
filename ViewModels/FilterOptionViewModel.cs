using CommunityToolkit.Mvvm.ComponentModel;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>Opción seleccionable en un grupo de chips de filtro (tipo de procedimiento, monto...).</summary>
public partial class FilterOptionViewModel : ObservableObject
{
    public FilterOptionViewModel(string label, object value, string? description = null)
    {
        Label = label;
        Value = value;
        Description = description ?? string.Empty;
    }

    public string Label { get; }

    public object Value { get; }

    /// <summary>Explicación breve que se muestra al seleccionar la opción.</summary>
    public string Description { get; }

    [ObservableProperty]
    private bool _isSelected;
}
