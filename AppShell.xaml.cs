using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Views;

namespace CompraPublicCercaMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(NavigationRoutes.Registro, typeof(RegisterPage));
        Routing.RegisterRoute(NavigationRoutes.RecuperarContrasena, typeof(ForgotPasswordPage));
        Routing.RegisterRoute(NavigationRoutes.ProcessDetail, typeof(ProcessDetailPage));
        Routing.RegisterRoute(NavigationRoutes.LocationPicker, typeof(LocationPickerPage));
    }
}
