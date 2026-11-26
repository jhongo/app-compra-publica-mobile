namespace CompraPublicCercaMobile.Models;

/// <summary>Detalle de un proceso de contratación (endpoint /record de SERCOP).</summary>
public sealed record ProcessDetail
{
    public required string Ocid { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string BuyerName { get; init; } = string.Empty;

    public string ProcurementType { get; init; } = string.Empty;

    /// <summary>Categoría OCDS: goods, works o services.</summary>
    public string? Category { get; init; }

    /// <summary>Estado OCDS: planning, active, complete, cancelled, unsuccessful...</summary>
    public string? Status { get; init; }

    public string? Locality { get; init; }

    public string? Region { get; init; }

    public decimal? Amount { get; init; }

    public string? Currency { get; init; }

    /// <summary>De dónde sale Amount: presupuesto de la convocatoria o valor adjudicado.</summary>
    public AmountSource AmountSource { get; init; }

    /// <summary>Fecha del release OCDS: última actualización del registro.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? TenderStart { get; init; }

    public DateTimeOffset? TenderEnd { get; init; }

    public int? NumberOfTenderers { get; init; }

    public IReadOnlyList<string> AwardedSuppliers { get; init; } = [];

    public IReadOnlyList<ProcessItem> Items { get; init; } = [];

    /// <summary>Resumen del proceso para listas (p. ej. al guardarlo como favorito desde el detalle).</summary>
    public Opportunity ToOpportunity() => new()
    {
        Ocid = Ocid,
        Code = Code,
        Description = Description,
        BuyerName = BuyerName,
        ProcurementType = ProcurementType,
        Locality = Locality,
        Region = Region,
        Amount = Amount,
        IsBudget = AmountSource == AmountSource.Tender,
        UpdatedAt = UpdatedAt
    };

    public string LocationText => DisplayFormat.Location(Locality, Region) ?? "No especificada";

    public string AmountText => DisplayFormat.Money(Amount, Currency) ?? "No publicado";

    public string UpdatedText => DisplayFormat.Date(UpdatedAt) ?? "—";

    public string AmountLabel => AmountSource switch
    {
        AmountSource.Tender => "Presupuesto referencial",
        AmountSource.Award => "Monto adjudicado",
        _ => "Monto"
    };

    public string TenderPeriodText => (TenderStart, TenderEnd) switch
    {
        ({ } start, { } end) => $"{DisplayFormat.Date(start)} – {DisplayFormat.Date(end)}",
        ({ } start, null) => $"Desde {DisplayFormat.Date(start)}",
        (null, { } end) => $"Hasta {DisplayFormat.Date(end)}",
        _ => "—"
    };

    public string TenderersText => NumberOfTenderers is { } n ? n.ToString(DisplayFormat.Culture) : "—";

    public string StatusText => Status switch
    {
        "planning" or "planned" => "En planificación",
        "active" => "En curso",
        "complete" => "Finalizado",
        "cancelled" => "Cancelado",
        "unsuccessful" => "Desierto",
        "withdrawn" => "Retirado",
        _ => "Sin estado"
    };

    public string CategoryText => Category switch
    {
        "goods" => "Bienes",
        "works" => "Obras",
        "services" => "Servicios",
        _ => "—"
    };

    public bool HasAwardedSuppliers => AwardedSuppliers.Count > 0;

    public bool HasItems => Items.Count > 0;
}

public sealed record ProcessItem(string Description, string QuantityText);

public enum AmountSource
{
    None,
    Tender,
    Award
}
