namespace CompraPublicCercaMobile.Constants;

/// <summary>
/// Rutas de Shell. Usar siempre estas constantes en lugar de escribir el texto a mano.
/// Las rutas raíz están en AppShell.xaml; las de detalle se registran en AppShell.xaml.cs.
/// </summary>
public static class NavigationRoutes
{
    public const string Login = "login";

    // Rutas de detalle (se apilan sobre la pantalla actual).
    public const string Registro = "registro";
    public const string RecuperarContrasena = "recuperar-contrasena";
    public const string ProcessDetail = "detalle";
    public const string LocationPicker = "ubicacion";

    // Pestañas de la barra de navegación inferior.
    public const string Inicio = "inicio";
    public const string Buscar = "buscar";
    public const string Favoritos = "favoritos";
    public const string Perfil = "perfil";

    // Parámetros de navegación.
    public const string OcidParameter = "ocid";
    public const string EmailParameter = "email";
}
