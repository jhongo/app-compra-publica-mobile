namespace CompraPublicCercaMobile.Constants;

/// <summary>
/// API propia (Laravel, proyecto admin-compra-publica): cuentas, login y perfil.
/// En desarrollo la sirve Laravel Valet en la Mac. La URL se elige sola según dónde corre la app.
/// </summary>
public static class BackendConstants
{
    /// <summary>
    /// Sitio de Valet. Solo lo resuelve la Mac (y el simulador de iOS, que usa su red);
    /// el certificado https es de la CA de Valet, que hay que instalar en el simulador
    /// (ver docs del API: «xcrun simctl keychain booted add-root-cert ...»).
    /// </summary>
    private const string ValetSite = "https://admin-compra-publica.test";

    /// <summary>
    /// IP de la Mac en el Wi-Fi, para un teléfono físico. Valet expone SOLO este proyecto
    /// en el puerto 8000 (archivo ~/.config/valet/Nginx/admin-compra-publica-lan).
    /// Si la Mac cambia de IP (otra red o el router la reasigna), actualízala aquí
    /// y en Platforms/Android/Resources/xml/network_security_config.xml.
    /// Para verla: «ipconfig getifaddr en0».
    /// </summary>
    private const string MacLanHost = "http://192.168.68.51:8000";

    /// <summary>
    /// Nombre Bonjour de la Mac («scutil --get LocalHostName» + «.local»), para un iPhone físico.
    /// NSAllowsLocalNetworking (ATS) permite http sin cifrar hacia nombres .local; hacia una IP
    /// iOS puede bloquearlo antes de enviar. Además no cambia aunque el router reasigne la IP.
    /// </summary>
    private const string MacBonjourHost = "http://MacBook-Pro-de-John.local:8000";

    /// <summary>El emulador de Android ve a la Mac como 10.0.2.2 (mismo bloque del puerto 8000).</summary>
    private const string AndroidEmulatorHost = "http://10.0.2.2:8000";

    /// <summary>Raíz de la API. Al desplegar, aquí va la URL https del servidor.</summary>
    public static string BaseUrl => Host + "/api/v1/";

    private static string Host
    {
        get
        {
            if (DeviceInfo.Current.DeviceType == DeviceType.Physical)
            {
#if IOS
                return MacBonjourHost;
#else
                // Android no resuelve nombres .local de forma fiable: va por IP.
                return MacLanHost;
#endif
            }

#if ANDROID
            return AndroidEmulatorHost;
#else
            return ValetSite;
#endif
        }
    }

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Largo del código que llega al correo.</summary>
    public const int VerificationCodeLength = 6;
}
