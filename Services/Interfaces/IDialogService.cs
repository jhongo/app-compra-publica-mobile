namespace CompraPublicCercaMobile.Services.Interfaces;

/// <summary>Diálogos del sistema sin que los ViewModels dependan de Shell o Page.</summary>
public interface IDialogService
{
    /// <returns>True si el usuario eligió <paramref name="accept"/>.</returns>
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
