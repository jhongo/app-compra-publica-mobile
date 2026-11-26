using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

public sealed class UserLocationService : IUserLocationService
{
    private const string ProvinceKey = "user_province";

    private readonly IPreferences _preferences;

    public UserLocationService(IPreferences preferences)
    {
        _preferences = preferences;

        // Descarta valores guardados que ya no sean una provincia válida.
        Province = EcuadorProvinces.Find(_preferences.Get<string?>(ProvinceKey, null));
    }

    public string? Province { get; private set; }

    public bool HasProvince => Province is not null;

    public void SetProvince(string province)
    {
        Province = EcuadorProvinces.Find(province)
            ?? throw new ArgumentException($"«{province}» no es una provincia del Ecuador.", nameof(province));
        _preferences.Set(ProvinceKey, Province);
    }

    public bool IsUserProvince(string? region) =>
        Province is not null && EcuadorProvinces.Find(region) == Province;
}
