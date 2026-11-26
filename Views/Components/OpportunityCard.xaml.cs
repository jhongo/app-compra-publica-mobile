using System.Windows.Input;

namespace CompraPublicCercaMobile.Views.Components;

public partial class OpportunityCard : ContentView
{
    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand), typeof(ICommand), typeof(OpportunityCard));

    public static readonly BindableProperty FavoriteCommandProperty = BindableProperty.Create(
        nameof(FavoriteCommand), typeof(ICommand), typeof(OpportunityCard));

    public OpportunityCard()
    {
        InitializeComponent();
    }

    /// <summary>Se ejecuta al tocar la tarjeta, con el OpportunityItemViewModel como parámetro.</summary>
    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    /// <summary>Se ejecuta al tocar la estrella, con el OpportunityItemViewModel como parámetro.</summary>
    public ICommand? FavoriteCommand
    {
        get => (ICommand?)GetValue(FavoriteCommandProperty);
        set => SetValue(FavoriteCommandProperty, value);
    }
}
