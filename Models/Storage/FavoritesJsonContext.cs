using System.Text.Json.Serialization;

namespace CompraPublicCercaMobile.Models.Storage;

/// <summary>
/// Serialización del archivo de favoritos, generada en compilación (sin reflexión).
/// IgnoreReadOnlyProperties evita guardar los textos calculados (AmountText, LocationText...).
/// </summary>
[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(List<Opportunity>))]
internal sealed partial class FavoritesJsonContext : JsonSerializerContext
{
}
