namespace CompraPublicCercaMobile.Models;

/// <summary>Una página de resultados y el total disponible en el servidor.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int Pages)
{
    public bool HasMorePages => Page < Pages;
}
