using System.Globalization;
using System.Text;

namespace CompraPublicCercaMobile.Constants;

/// <summary>Las 24 provincias del Ecuador, escritas como se muestran al usuario.</summary>
public static class EcuadorProvinces
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Azuay",
        "Bolívar",
        "Cañar",
        "Carchi",
        "Chimborazo",
        "Cotopaxi",
        "El Oro",
        "Esmeraldas",
        "Galápagos",
        "Guayas",
        "Imbabura",
        "Loja",
        "Los Ríos",
        "Manabí",
        "Morona Santiago",
        "Napo",
        "Orellana",
        "Pastaza",
        "Pichincha",
        "Santa Elena",
        "Santo Domingo de los Tsáchilas",
        "Sucumbíos",
        "Tungurahua",
        "Zamora Chinchipe"
    ];

    /// <summary>
    /// Busca la provincia ignorando mayúsculas y tildes: "LOS RIOS" -> "Los Ríos".
    /// Devuelve null si el texto no es una provincia.
    /// </summary>
    public static string? Find(string? name)
    {
        var key = Normalize(name);
        return key.Length == 0 ? null : All.FirstOrDefault(p => Normalize(p) == key);
    }

    // "Los Ríos" y "LOS RIOS" -> "LOS RIOS"; "Cañar" y "CAÑAR" -> "CANAR".
    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
