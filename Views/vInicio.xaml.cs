using CompraPublicCercaMobile.ViewModels;

namespace CompraPublicCercaMobile.Views;

public partial class vInicio : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public vInicio(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // La pestaña se reutiliza: el ViewModel decide si ya cargó los datos.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
