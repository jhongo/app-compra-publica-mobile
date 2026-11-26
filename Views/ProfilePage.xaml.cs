using CompraPublicCercaMobile.ViewModels;

namespace CompraPublicCercaMobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // La pestaña se reutiliza entre sesiones: se refresca el usuario al mostrarla.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProfile();
    }
}
