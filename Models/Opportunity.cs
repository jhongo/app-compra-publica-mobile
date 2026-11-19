namespace CompraPublicCercaMobile.Models;

/// <summary>Oportunidad de compra pública recomendada al proveedor.</summary>
/// <param name="MatchPercentage">Porcentaje de coincidencia con el perfil del proveedor (0-100).</param>
public sealed record Opportunity(string Id, string Title, int MatchPercentage, DateTime Deadline);
