namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>Token de la API guardado de forma cifrada por el sistema (Keychain en iOS, Keystore en Android).</summary>
public interface ITokenStore
{
    Task<string?> GetAsync();

    Task SetAsync(string token);

    void Clear();
}
