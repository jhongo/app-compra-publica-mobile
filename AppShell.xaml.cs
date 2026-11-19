using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Views;

namespace CompraPublicCercaMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(NavigationRoutes.Registro, typeof(RegisterPage));
    }
}
