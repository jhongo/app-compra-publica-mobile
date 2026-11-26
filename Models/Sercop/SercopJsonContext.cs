using System.Text.Json.Serialization;

namespace CompraPublicCercaMobile.Models.Sercop;

/// <summary>
/// Serialización generada en compilación: evita la reflexión, que el recorte
/// de código (trimming) de iOS/Android en Release puede romper.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(SercopSearchResponse))]
[JsonSerializable(typeof(SercopRecordResponse))]
internal sealed partial class SercopJsonContext : JsonSerializerContext
{
}
