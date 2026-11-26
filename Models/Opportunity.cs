using System.Globalization;

namespace CompraPublicCercaMobile.Models;

/// <summary>Proceso de contratación pública tal como lo muestra la app en listas.</summary>
public sealed record Opportunity
{
    public required string Ocid { get; init; }

    /// <summary>Código del proceso en SERCOP (p. ej. "SIE-PH-2026-001-375709").</summary>
    public string Code { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string BuyerName { get; init; } = string.Empty;

    /// <summary>Tipo de procedimiento (p. ej. "Subasta Inversa Electrónica").</summary>
    public string ProcurementType { get; init; } = string.Empty;

    public string? Locality { get; init; }

    public string? Region { get; init; }

    public decimal? Amount { get; init; }

    /// <summary>True si Amount es el presupuesto del proceso; false si es el monto de la API
    /// sin presupuesto (en subastas suele ser el valor adjudicado o del contrato).</summary>
    public bool IsBudget { get; init; }

    /// <summary>Fecha del registro en SERCOP: es su última actualización, no la de publicación.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>True si el proceso es de la provincia del usuario.</summary>
    public bool IsNearby { get; init; }

    public string LocationText => DisplayFormat.Location(Locality, Region) ?? "Ubicación no especificada";

    public string AmountText => DisplayFormat.Money(Amount) ?? "Monto no publicado";

    public string AmountLabel => IsBudget ? "Presupuesto" : "Monto";

    public string UpdatedText => UpdatedAt is { } date
        ? $"Actualizado el {date.ToString("dd MMM yyyy", DisplayFormat.Culture)}"
        : string.Empty;
}

/// <summary>Formatos de texto compartidos por los modelos de procesos.</summary>
internal static class DisplayFormat
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-EC");

    public static string? Money(decimal? amount, string? currency = "USD") =>
        amount is > 0 ? $"{amount.Value.ToString("C2", Culture)} {currency ?? "USD"}".TrimEnd() : null;

    public static string? Location(string? locality, string? region)
    {
        var parts = new[] { locality, region }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Culture.TextInfo.ToTitleCase(p!.Trim().ToLower(Culture)))
            .Distinct()
            .ToArray();
        return parts.Length == 0 ? null : string.Join(", ", parts);
    }

    public static string? Date(DateTimeOffset? date) =>
        date?.ToString("dd MMM yyyy", Culture);

    /// <summary>
    /// SERCOP publica muchas descripciones TODO EN MAYÚSCULAS, que cuestan leer en
    /// pantalla. Si el texto está casi todo en mayúsculas, lo pasa a formato oración.
    /// </summary>
    public static string SentenceCase(string text)
    {
        var letters = text.Where(char.IsLetter).ToList();
        if (letters.Count == 0 || letters.Count(char.IsUpper) < letters.Count * 0.7)
        {
            return text;
        }

        var lower = text.ToLower(Culture);
        return char.ToUpper(lower[0], Culture) + lower[1..];
    }
}
