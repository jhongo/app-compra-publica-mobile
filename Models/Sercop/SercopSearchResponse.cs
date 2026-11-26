using System.Text.Json.Serialization;

namespace CompraPublicCercaMobile.Models.Sercop;

// DTOs de la respuesta de /search_ocds. Reflejan el JSON tal cual; la app usa
// Opportunity, que se arma a partir de estos en SercopProcessService.

public sealed class SercopSearchResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    // La API la devuelve a veces como número y a veces como texto ("1").
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("data")]
    public List<SercopProcessDto> Data { get; set; } = new();
}

/// <summary>
/// Un proceso dentro de la búsqueda. Incluye los nombres de campo de la documentación
/// (buyerName, single_provider) y los que devuelve hoy la API (buyer, suppliers).
/// </summary>
public sealed class SercopProcessDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("ocid")]
    public string? Ocid { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("internal_type")]
    public string? InternalType { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("buyer")]
    public string? Buyer { get; set; }

    [JsonPropertyName("buyerName")]
    public string? BuyerName { get; set; }

    [JsonPropertyName("suppliers")]
    public string? Suppliers { get; set; }

    [JsonPropertyName("single_provider")]
    public string? SingleProvider { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    // Llegan como texto ("42000.000000"); se leen como número con AllowReadingFromString.
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("budget")]
    public decimal? Budget { get; set; }
}
