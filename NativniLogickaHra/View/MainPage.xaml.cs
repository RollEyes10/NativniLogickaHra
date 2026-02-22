namespace NativniLogickaHra
{
    using NativniLogickaHra.View;

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnStartGameClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new Game());

        private async void OnSettingsClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new SettingsPage());

        private async void OnAboutClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new ConnectionAI());
    }
}