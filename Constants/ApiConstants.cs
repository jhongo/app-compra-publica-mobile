namespace CompraPublicCercaMobile.Constants;

/// <summary>
/// API pública de datos abiertos de SERCOP (estándar OCDS). No requiere credenciales.
/// Documentación: https://datosabiertos.compraspublicas.gob.ec/PLATAFORMA/datos-abiertos/api
/// </summary>
public static class ApiConstants
{
    public const string SercopBaseUrl = "https://datosabiertos.compraspublicas.gob.ec/PLATAFORMA/api/";

    /// <summary>Búsqueda por palabra clave. Parámetros: year y search obligatorios; page, buyer y supplier opcionales.</summary>
    public const string SearchEndpoint = "search_ocds";

    /// <summary>Detalle de un proceso. Parámetro: ocid.</summary>
    public const string RecordEndpoint = "record";

    /// <summary>La API exige al menos tres caracteres en search, buyer y supplier.</summary>
    public const int MinSearchLength = 3;

    /// <summary>Primer año con datos en la API.</summary>
    public const int FirstAvailableYear = 2015;

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);
}
