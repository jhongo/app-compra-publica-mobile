using System.Globalization;
using System.Net.Http.Json;
using CompraPublicCercaMobile.Constants;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Models.Sercop;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

/// <summary>
/// Implementación de <see cref="IProcessService"/> con la API de datos abiertos de SERCOP.
/// Convierte los DTOs de la API en los modelos de la app.
/// </summary>
public sealed class SercopProcessService : IProcessService
{
    private readonly HttpClient _httpClient;

    public SercopProcessService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<Opportunity>> SearchAsync(
        ProcessSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var url = BuildSearchUrl(query);
        var response = await GetWithRetryAsync(
            url, SercopJsonContext.Default.SercopSearchResponse, cancellationToken);

        if (response is null)
        {
            return new PagedResult<Opportunity>([], 0, query.Page, 0);
        }

        var items = response.Data
            .Where(dto => !string.IsNullOrWhiteSpace(dto.Ocid))
            .Select(MapOpportunity)
            .ToList();

        return new PagedResult<Opportunity>(items, response.Total, Math.Max(response.Page, query.Page), response.Pages);
    }

    public async Task<FilteredSearchResult> SearchFilteredAsync(
        ProcessSearchQuery query,
        LocalProcessFilter filter,
        int minResults = 10,
        int maxPages = 5,
        CancellationToken cancellationToken = default)
    {
        var matches = new List<Opportunity>();
        var page = Math.Max(query.Page, 1);
        var lastPage = page - 1;
        var total = 0;
        var pages = 0;
        var scanned = 0;

        for (var i = 0; i < Math.Max(maxPages, 1); i++)
        {
            var result = await SearchAsync(query with { Page = page }, cancellationToken);
            total = result.Total;
            pages = result.Pages;
            lastPage = result.Page;
            scanned += result.Items.Count;
            matches.AddRange(result.Items.Where(o => Matches(o, filter)));

            if (!result.HasMorePages || matches.Count >= minResults)
            {
                break;
            }

            page = result.Page + 1;
        }

        return new FilteredSearchResult(matches, total, lastPage, pages, scanned);
    }

    private static bool Matches(Opportunity opportunity, LocalProcessFilter filter)
    {
        if (filter.Province is not null && EcuadorProvinces.Find(opportunity.Region) != filter.Province)
        {
            return false;
        }

        if (filter.AmountRange == AmountRange.Any)
        {
            return true;
        }

        // Con filtro de monto, los procesos sin monto publicado no se pueden clasificar.
        return opportunity.Amount is { } amount && filter.AmountRange switch
        {
            AmountRange.UpTo5K => amount <= 5_000m,
            AmountRange.From5KTo10K => amount > 5_000m && amount <= 10_000m,
            AmountRange.From10KTo50K => amount > 10_000m && amount <= 50_000m,
            AmountRange.Over50K => amount > 50_000m,
            _ => true
        };
    }

    private static string BuildSearchUrl(ProcessSearchQuery query)
    {
        var keyword = RequireMinLength(query.Keyword, "La palabra clave", nameof(query.Keyword))!;
        var buyer = RequireMinLength(query.Buyer, "La entidad", nameof(query.Buyer));
        var supplier = RequireMinLength(query.Supplier, "El proveedor", nameof(query.Supplier));
        var year = Math.Clamp(query.Year, ApiConstants.FirstAvailableYear, DateTime.Today.Year);
        var page = Math.Max(query.Page, 1);

        var url = $"{ApiConstants.SearchEndpoint}?year={year}&search={Uri.EscapeDataString(keyword)}&page={page}";
        if (buyer is not null)
        {
            url += $"&buyer={Uri.EscapeDataString(buyer)}";
        }

        if (supplier is not null)
        {
            url += $"&supplier={Uri.EscapeDataString(supplier)}";
        }

        var method = query.Method switch
        {
            ProcurementMethod.Open => "open",
            ProcurementMethod.Selective => "selective",
            ProcurementMethod.Direct => "direct",
            _ => null
        };
        if (method is not null)
        {
            url += $"&method={method}";
        }

        return url;
    }

    // La API exige al menos 3 caracteres en search, buyer y supplier.
    // Keyword es obligatoria; los demás se omiten si vienen vacíos.
    private static string? RequireMinLength(string? value, string label, string paramName)
    {
        value = value?.Trim();
        if (string.IsNullOrEmpty(value) && paramName != nameof(ProcessSearchQuery.Keyword))
        {
            return null;
        }

        if (value is null || value.Length < ApiConstants.MinSearchLength)
        {
            throw new ArgumentException(
                $"{label} necesita al menos {ApiConstants.MinSearchLength} caracteres.", paramName);
        }

        return value;
    }

    // La API a veces responde con un error momentáneo o con contenido que no es JSON:
    // se reintenta una vez antes de informar el error.
    private async Task<T?> GetWithRetryAsync<T>(
        string url,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync(url, typeInfo, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException
                                   && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return await _httpClient.GetFromJsonAsync(url, typeInfo, cancellationToken);
        }
    }

    public async Task<ProcessDetail?> GetDetailAsync(string ocid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ocid))
        {
            return null;
        }

        var url = $"{ApiConstants.RecordEndpoint}?ocid={Uri.EscapeDataString(ocid.Trim())}";
        var response = await GetWithRetryAsync(
            url, SercopJsonContext.Default.SercopRecordResponse, cancellationToken);

        var releases = response?.Releases
            ?? response?.Records?.SelectMany(r => r.Releases ?? []).ToList();

        // Un record puede traer varias versiones (releases); la más reciente va al final.
        var release = releases?.LastOrDefault();
        return release is null ? null : MapDetail(ocid, release);
    }

    private static Opportunity MapOpportunity(SercopProcessDto dto) => new()
    {
        Ocid = dto.Ocid!,
        Code = dto.Title ?? string.Empty,
        Description = DisplayFormat.SentenceCase(Clean(dto.Description) ?? dto.Title ?? string.Empty),
        BuyerName = Clean(dto.Buyer ?? dto.BuyerName) ?? "Entidad no especificada",
        ProcurementType = Clean(dto.InternalType) ?? "Proceso de contratación",
        Locality = Clean(dto.Locality),
        // La API escribe "LOS RIOS"; se muestra "Los Ríos" si es una provincia conocida.
        Region = EcuadorProvinces.Find(dto.Region) ?? Clean(dto.Region),
        Amount = dto.Budget ?? dto.Amount,
        IsBudget = dto.Budget is not null,
        UpdatedAt = ParseDate(dto.Date)
    };

    private static ProcessDetail MapDetail(string ocid, OcdsRelease release)
    {
        var tender = release.Tender;
        var buyerParty = release.Parties?.FirstOrDefault(p => p.Roles?.Contains("buyer") == true);
        var award = release.Awards?.FirstOrDefault();

        return new ProcessDetail
        {
            Ocid = release.Ocid ?? ocid,
            Code = tender?.Title ?? string.Empty,
            Description = DisplayFormat.SentenceCase(
                Clean(tender?.Description) ?? Clean(release.Planning?.Rationale) ?? tender?.Title ?? string.Empty),
            BuyerName = Clean(release.Buyer?.Name) ?? Clean(buyerParty?.Name) ?? "Entidad no especificada",
            ProcurementType = Clean(tender?.ProcurementMethodDetails) ?? "Proceso de contratación",
            Category = tender?.MainProcurementCategory,
            Status = tender?.Status,
            Locality = Clean(buyerParty?.Address?.Locality),
            Region = EcuadorProvinces.Find(buyerParty?.Address?.Region) ?? Clean(buyerParty?.Address?.Region),
            Amount = tender?.Value?.Amount ?? award?.Value?.Amount,
            Currency = tender?.Value?.Currency ?? award?.Value?.Currency,
            AmountSource = tender?.Value?.Amount is not null ? AmountSource.Tender
                : award?.Value?.Amount is not null ? AmountSource.Award
                : AmountSource.None,
            UpdatedAt = ParseDate(release.Date),
            TenderStart = ParseDate(tender?.TenderPeriod?.StartDate),
            TenderEnd = ParseDate(tender?.TenderPeriod?.EndDate),
            NumberOfTenderers = tender?.NumberOfTenderers,
            AwardedSuppliers = release.Awards?
                .SelectMany(a => a.Suppliers ?? [])
                .Select(s => Clean(s.Name))
                .OfType<string>()
                .Distinct()
                .ToList() ?? [],
            Items = tender?.Items?
                .Where(i => !string.IsNullOrWhiteSpace(i.Description))
                .Select(i => new ProcessItem(DisplayFormat.SentenceCase(Clean(i.Description)!), FormatQuantity(i)))
                .ToList() ?? []
        };
    }

    private static string FormatQuantity(OcdsItem item)
    {
        var quantity = item.Quantity?.ToString("0.##", DisplayFormat.Culture);
        var unit = Clean(item.Unit?.Name)?.ToLower(DisplayFormat.Culture);
        return string.Join(" ", new[] { quantity, unit }.Where(p => !string.IsNullOrEmpty(p)));
    }

    // Quita espacios repetidos y saltos de línea que trae la API en las descripciones.
    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static DateTimeOffset? ParseDate(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date)
            ? date
            : null;
}
