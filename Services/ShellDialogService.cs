using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

public sealed class ShellDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        var page = Shell.Current?.CurrentPage
            ?? throw new InvalidOperationException("No hay una pantalla activa para mostrar el diálogo.");
        return page.DisplayAlertAsync(title, message, accept, cancel);
    }
}
