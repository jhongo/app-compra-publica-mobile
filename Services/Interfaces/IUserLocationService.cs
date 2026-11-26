namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>
/// Provincia del usuario. Se elige en el registro o desde Inicio y se guarda en el
/// dispositivo, así que se mantiene al cerrar la app.
/// </summary>
public interface IUserLocationService
{
    /// <summary>Provincia elegida (p. ej. "Los Ríos"), o null si aún no eligió.</summary>
    string? Province { get; }

    bool HasProvince { get; }

    /// <summary>Guarda la provincia. Debe ser una de <see cref="Constants.EcuadorProvinces.All"/>.</summary>
    void SetProvince(string province);

    /// <summary>
    /// True si la región que devuelve SERCOP (p. ej. "LOS RIOS") es la provincia del usuario.
    /// Ignora mayúsculas y tildes.
    /// </summary>
    bool IsUserProvince(string? region);
}
