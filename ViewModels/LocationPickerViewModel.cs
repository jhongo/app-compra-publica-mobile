using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.ViewModels;

/// <summary>Lista de provincias con buscador para cambiar la ubicación del usuario.</summary>
public partial class LocationPickerViewModel : BaseViewModel
{
    private readonly IUserLocationService _location;
    private readonly IAccountService _account;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<ProvinceItemViewModel> Provinces { get; } = new();

    public LocationPickerViewModel(IUserLocationService location, IAccountService account, INavigationService navigation)
    {
        _location = location;
        _account = account;
        _navigation = navigation;
        Title = "Tu provincia";
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task SelectAsync(ProvinceItemViewModel province)
    {
        _location.SetProvince(province.Name);
        // También se guarda en la cuenta, sin hacer esperar al usuario.
        _ = _account.SyncProvinceAsync(province.Name);
        await _navigation.GoToAsync("..");
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigation.GoToAsync("..");

    // Ignora mayúsculas y tildes: "rios" encuentra "Los Ríos".
    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        var compare = CultureInfo.InvariantCulture.CompareInfo;
        const CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        Provinces.Clear();
        foreach (var name in EcuadorProvinces.All)
        {
            if (query.Length == 0 || compare.IndexOf(name, query, options) >= 0)
            {
                Provinces.Add(new ProvinceItemViewModel(name, name == _location.Province));
            }
        }
    }
}

public sealed class ProvinceItemViewModel
{
    public ProvinceItemViewModel(string name, bool isSelected)
    {
        Name = name;
        IsSelected = isSelected;
    }

    public string Name { get; }

    public bool IsSelected { get; }
}
