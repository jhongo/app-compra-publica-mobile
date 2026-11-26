using System.Text.Json.Serialization;

namespace CompraPublicCercaMobile.Models.Sercop;

// DTOs de /record (estándar OCDS). Solo se mapean los campos que usa la app.
// La documentación muestra records[].releases[], pero la API devuelve hoy
// releases[] en la raíz: se aceptan ambas formas.

public sealed class SercopRecordResponse
{
    [JsonPropertyName("releases")]
    public List<OcdsRelease>? Releases { get; set; }

    [JsonPropertyName("records")]
    public List<OcdsRecord>? Records { get; set; }
}

public sealed class OcdsRecord
{
    [JsonPropertyName("releases")]
    public List<OcdsRelease>? Releases { get; set; }
}

public sealed class OcdsRelease
{
    [JsonPropertyName("ocid")]
    public string? Ocid { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("buyer")]
    public OcdsOrganization? Buyer { get; set; }

    [JsonPropertyName("tender")]
    public OcdsTender? Tender { get; set; }

    [JsonPropertyName("parties")]
    public List<OcdsParty>? Parties { get; set; }

    [JsonPropertyName("awards")]
    public List<OcdsAward>? Awards { get; set; }

    [JsonPropertyName("planning")]
    public OcdsPlanning? Planning { get; set; }
}

public sealed class OcdsOrganization
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class OcdsTender
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("procurementMethodDetails")]
    public string? ProcurementMethodDetails { get; set; }

    [JsonPropertyName("mainProcurementCategory")]
    public string? MainProcurementCategory { get; set; }

    [JsonPropertyName("value")]
    public OcdsValue? Value { get; set; }

    [JsonPropertyName("tenderPeriod")]
    public OcdsPeriod? TenderPeriod { get; set; }

    [JsonPropertyName("enquiryPeriod")]
    public OcdsPeriod? EnquiryPeriod { get; set; }

    [JsonPropertyName("numberOfTenderers")]
    public int? NumberOfTenderers { get; set; }

    [JsonPropertyName("items")]
    public List<OcdsItem>? Items { get; set; }
}

public sealed class OcdsValue
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

public sealed class OcdsPeriod
{
    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }
}

public sealed class OcdsItem
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("unit")]
    public OcdsUnit? Unit { get; set; }
}

public sealed class OcdsUnit
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class OcdsParty
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    [JsonPropertyName("address")]
    public OcdsAddress? Address { get; set; }
}

public sealed class OcdsAddress
{
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }
}

public sealed class OcdsAward
{
    [JsonPropertyName("value")]
    public OcdsValue? Value { get; set; }

    [JsonPropertyName("suppliers")]
    public List<OcdsOrganization>? Suppliers { get; set; }
}

public sealed class OcdsPlanning
{
    [JsonPropertyName("rationale")]
    public string? Rationale { get; set; }
}
