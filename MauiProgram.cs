
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Controls;
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

        // BorderlessEntry / BorderlessDatePicker: quitan el borde nativo para que
        // solo se vea el Border del diseño.
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
        {
            if (view is not BorderlessEntry)
            {
                return;
            }
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
        {
            if (view is not BorderlessPicker)
            {
                return;
            }
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
        {
            if (view is not BorderlessDatePicker)
            {
                return;
            }
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

#if ANDROID
        builder.Services.AddSingleton<IAuthService, AndroidAuthService>();
        builder.Services.AddSingleton<IBiometricService, AndroidBiometricService>();
#elif IOS
        builder.Services.AddSingleton<IAuthService, IosAuthService>();
        builder.Services.AddSingleton<IBiometricService, IosBiometricService>();
#else
        builder.Services.AddSingleton<IAuthService, UnsupportedAuthService>();
        builder.Services.AddSingleton<IBiometricService, UnsupportedBiometricService>();
#endif
        builder.Services.AddSingleton<ISessionService, SessionService>();

        // API propia (Laravel): cuentas, login y perfil. Token en el almacenamiento seguro del sistema.
        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddSingleton(DeviceInfo.Current);
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();
        builder.Services.AddSingleton<IBackendAuthClient>(_ => new BackendAuthClient(new HttpClient
        {
            BaseAddress = new Uri(BackendConstants.BaseUrl),
            Timeout = BackendConstants.RequestTimeout
        }));
        builder.Services.AddSingleton<IAccountService, AccountService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();
        builder.Services.AddSingleton(AppInfo.Current);

        // Provincia del usuario, guardada en el dispositivo.
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton<IUserLocationService, UserLocationService>();

        // Favoritos, guardados en un archivo JSON en la carpeta privada de la app.
        builder.Services.AddSingleton(FileSystem.Current);
        builder.Services.AddSingleton<IFavoritesService, FavoritesService>();

        // API de datos abiertos de SERCOP: un solo HttpClient para toda la app.
        builder.Services.AddSingleton<IProcessService>(_ => new SercopProcessService(new HttpClient
        {
            BaseAddress = new Uri(ApiConstants.SercopBaseUrl),
            Timeout = ApiConstants.RequestTimeout
        }));

        builder.Services.AddSingleton<AppShell>();

        // Views y ViewModels: Transient (docs/arquitectura.md, decisión 2).
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();


        // Inicio y pestañas de la barra de navegación
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<vInicio>();

        // Selección de provincia
        builder.Services.AddTransient<LocationPickerViewModel>();
        builder.Services.AddTransient<LocationPickerPage>();

        // Detalle de proceso
        builder.Services.AddTransient<ProcessDetailViewModel>();
        builder.Services.AddTransient<ProcessDetailPage>();

        // Búsqueda
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<SearchPage>();

        // Favoritos
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<FavoritesPage>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();

        // Registro
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        // Recuperar contraseña
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ForgotPasswordPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
