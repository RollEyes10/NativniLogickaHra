namespace NativniLogickaHra.View;

public partial class Instruction : ContentPage
{
    public Instruction()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnOpenLink(object sender, TappedEventArgs e)
    {
        var url = e.Parameter as string;
        if (!string.IsNullOrEmpty(url))
            await Launcher.OpenAsync(url);
    }
}
