using CompraPublicCercaMobile.ViewModels;

namespace CompraPublicCercaMobile.Views;

public partial class ProcessDetailPage : ContentPage
{
    public ProcessDetailPage(ProcessDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
