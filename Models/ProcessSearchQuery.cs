namespace CompraPublicCercaMobile.Models;

/// <summary>
/// Filtros que aplica la API de SERCOP en el servidor (/search_ocds).
/// Keyword y Year son obligatorios; Buyer y Supplier necesitan al menos 3 caracteres.
/// </summary>
public sealed record ProcessSearchQuery(string Keyword, int Year)
{
    public int Page { get; init; } = 1;

    /// <summary>Palabra en el nombre de la entidad contratante (p. ej. "QUITO").</summary>
    public string? Buyer { get; init; }

    /// <summary>Palabra en el nombre del proveedor.</summary>
    public string? Supplier { get; init; }

    /// <summary>Tipo de procedimiento OCDS. La API lo acepta aunque no esté documentado.</summary>
    public ProcurementMethod Method { get; init; } = ProcurementMethod.Any;
}

/// <summary>Métodos de contratación del estándar OCDS (parámetro "method").</summary>
public enum ProcurementMethod
{
    Any,

    /// <summary>open: licitación, subasta inversa, cotización...</summary>
    Open,

    /// <summary>selective: catálogo electrónico con mejor oferta.</summary>
    Selective,

    /// <summary>direct: compra directa por catálogo, ínfima cuantía...</summary>
    Direct
}

/// <summary>
/// Filtros que la API no ofrece y que la app aplica sobre las páginas que descarga.
/// </summary>
public sealed record LocalProcessFilter
{
    public static LocalProcessFilter None { get; } = new();

    /// <summary>Provincia (como en EcuadorProvinces.All), o null para todas.</summary>
    public string? Province { get; init; }

    public AmountRange AmountRange { get; init; } = AmountRange.Any;

    public bool IsActive => Province is not null || AmountRange != AmountRange.Any;
}

public enum AmountRange
{
    Any,
    UpTo5K,
    From5KTo10K,
    From10KTo50K,
    Over50K
}

/// <summary>
/// Resultado de una búsqueda con filtros locales: además de los procesos que cumplen,
/// informa cuántos se revisaron para ser transparentes con el usuario.
/// </summary>
public sealed record FilteredSearchResult(
    IReadOnlyList<Opportunity> Items,
    int Total,
    int LastPage,
    int Pages,
    int ScannedCount)
{
    public bool HasMorePages => LastPage < Pages;
}
