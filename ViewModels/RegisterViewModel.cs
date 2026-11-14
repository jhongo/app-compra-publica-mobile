
using CommunityToolkit.Mvvm.ComponentModel;

namespace CompraPublicCercaMobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        [ObservableProperty]
        private string firstNames = string.Empty;

        [ObservableProperty]
        private string lastNames = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string verificationCode = string.Empty;

        [ObservableProperty]
        private string address = string.Empty;

        [ObservableProperty]
        private string postalCode = string.Empty;

        [ObservableProperty]
        private DateTime birthDate = new DateTime(2000, 1, 1);
    }
}
