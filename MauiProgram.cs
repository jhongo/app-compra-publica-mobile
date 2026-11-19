
using CompraPublicCercaMobile.Services;
using CompraPublicCercaMobile.Services.Interfaces;
using CompraPublicCercaMobile.ViewModels;
using CompraPublicCercaMobile.Views;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        builder.Services.AddSingleton<IAuthService, AndroidAuthService>();
#elif IOS
        builder.Services.AddSingleton<IAuthService, IosAuthService>();
#else
        builder.Services.AddSingleton<IAuthService, UnsupportedAuthService>();
#endif
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        builder.Services.AddSingleton<AppShell>();

        // Views y ViewModels: Transient (docs/arquitectura.md, decisión 2).
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        // Inicio y pestañas de la barra de navegación
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<vInicio>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<ProfilePage>();

        // Registro
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
