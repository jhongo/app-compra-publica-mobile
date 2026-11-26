using CompraPublicCercaMobile.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraPublicCercaMobile.Services;

public sealed class SecureTokenStore : ITokenStore
{
    private const string Key = "api_token";

    private readonly ISecureStorage _storage;
    private readonly ILogger<SecureTokenStore> _logger;

    public SecureTokenStore(ISecureStorage storage, ILogger<SecureTokenStore> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<string?> GetAsync()
    {
        try
        {
            return await _storage.GetAsync(Key);
        }
        catch (Exception ex)
        {
            // Puede fallar si el sistema rotó las claves (p. ej. al restaurar un respaldo):
            // se trata como "sin sesión guardada" y el usuario vuelve a iniciar sesión.
            _logger.LogWarning(ex, "No se pudo leer el token guardado.");
            _storage.Remove(Key);
            return null;
        }
    }

    public Task SetAsync(string token) => _storage.SetAsync(Key, token);

    public void Clear() => _storage.Remove(Key);
}
