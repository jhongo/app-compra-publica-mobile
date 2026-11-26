using CompraPublicCercaMobile.ViewModels;

namespace CompraPublicCercaMobile.Views;

public partial class LocationPickerPage : ContentPage
{
    public LocationPickerPage(LocationPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
