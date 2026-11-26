using System.Windows.Input;

namespace CompraPublicCercaMobile.Views.Components;

public partial class ProfileRow : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ProfileRow));

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ProfileRow), string.Empty);

    public static readonly BindableProperty DetailProperty =
        BindableProperty.Create(nameof(Detail), typeof(string), typeof(ProfileRow), string.Empty);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ProfileRow));

    public static readonly BindableProperty ShowChevronProperty =
        BindableProperty.Create(nameof(ShowChevron), typeof(bool), typeof(ProfileRow), true);

    public ProfileRow()
    {
        InitializeComponent();
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Valor actual a la derecha (p. ej. la provincia) o "Próximamente".</summary>
    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Solo las filas que llevan a otra pantalla muestran el chevron.</summary>
    public bool ShowChevron
    {
        get => (bool)GetValue(ShowChevronProperty);
        set => SetValue(ShowChevronProperty, value);
    }
}
