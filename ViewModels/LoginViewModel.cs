using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInWithGoogleCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Inicia sesión para continuar.";

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    private bool CanSignIn() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    private async Task SignInWithGoogleAsync()
    {
        IsBusy = true;
        StatusMessage = "Conectando con Google…";

        try
        {
            var user = await _authService.SignInWithGoogleAsync();
            StatusMessage = user is null
                ? "Inicio de sesión cancelado."
                : $"Hola, {user.DisplayName}\n{user.Email}\nUID: {user.Uid}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
