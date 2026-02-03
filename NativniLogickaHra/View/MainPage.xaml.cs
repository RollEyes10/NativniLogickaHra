namespace NativniLogickaHra
{
    using NativniLogickaHra.View;
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }


        // Obsluha tlačítka Start Game
        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            // Zde můžeš navigovat na stránku s hrou nebo vypsat upozornění
            await Navigation.PushAsync(new Game());
        }

        // Obsluha tlačítka Settings
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Přepne uživatele na stránku nastavení
            await Navigation.PushAsync(new SettingsPage());
        }

        // Obsluha tlačítka About
        private async void OnAboutClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConnectionAI());
        }
    }
}