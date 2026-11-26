using CompraPublicCercaMobile.ViewModels;

namespace CompraPublicCercaMobile.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
