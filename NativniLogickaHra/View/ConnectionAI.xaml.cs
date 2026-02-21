using System.Collections.ObjectModel;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.View;

public partial class ConnectionAI : ContentPage
{
    private readonly string[] Providers = { "Gemini", "ChatGPT", "Claude" };
    private ObservableCollection<ApiConnectionItem> Connections = new();

    public ConnectionAI()
    {
        InitializeComponent();
        ProviderPicker.SelectedIndex = 0;
        LoadConnections();
    }
    private async void OnInstructionClicked(object sender, EventArgs e)
    {
        Logger.Log("Instruction clicked");
        await Navigation.PushAsync(new Instruction());
    }


    // ===== MODELY =====
    private class ApiConnectionItem
    {
        public string Provider { get; set; } = "";
        public string Status { get; set; } = "";
    }

    // ===== NAČTENÍ SEZNAMU =====
    private async void LoadConnections()
    {
        Logger.Log("Loading connections list");
        Connections.Clear();

        foreach (var provider in Providers)
        {
            var key = await SecureStorage.Default.GetAsync(provider);
            var mode = await SecureStorage.Default.GetAsync($"{provider}_Mode");

            Connections.Add(new ApiConnectionItem
            {
                Provider = provider,
                Status = string.IsNullOrEmpty(key)
                    ? "❌ Bez klíče"
                    : provider == "Gemini"
                        ? $"✅ Uložen ({mode ?? "FREE"})"
                        : "✅ Uložen"
            });
        }

        ConnectionsList.ItemsSource = Connections;
    }

    // ===== VÝBĚR ZE SEZNAMU =====
    private async void OnConnectionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ApiConnectionItem item)
            return;

        Logger.Log($"Connection selected: {item.Provider}");
        ProviderPicker.SelectedItem = item.Provider;
        await LoadSelectedProvider();
    }

    private async Task LoadSelectedProvider()
    {
        string provider = ProviderPicker.SelectedItem?.ToString() ?? "Gemini";

        // persist currently selected AI provider so other pages (hra) can use it
        Preferences.Default.Set("SelectedAIProvider", provider);

        GeminiModePanel.IsVisible = provider == "Gemini";

        ApiKeyEntry.Text =
            await SecureStorage.Default.GetAsync(provider) ?? "";

        if (provider == "Gemini")
        {
            string? mode = await SecureStorage.Default.GetAsync($"{provider}_Mode");
            PaidCheckBox.IsChecked = mode == "PAID";
        }

        StatusLabel.Text = $"Editace připojení: {provider}";
    }

    private async void OnProviderChanged(object sender, EventArgs e)
    {
        var provider = ProviderPicker.SelectedItem?.ToString() ?? "Gemini";
        Logger.Log($"Provider changed to: {provider}");
        Preferences.Default.Set("SelectedAIProvider", provider); // nutné pro hru
        await LoadSelectedProvider();
    }

    // ===== ULOŽENÍ =====
    private async void OnSaveKeyClicked(object sender, EventArgs e)
    {
        string provider = ProviderPicker.SelectedItem?.ToString() ?? "Gemini";
        string key = ApiKeyEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(key))
        {
            await DisplayAlertAsync("Chyba", "Zadejte API klíč.", "OK");
            return;
        }

        await SecureStorage.Default.SetAsync(provider, key);

        if (provider == "Gemini")
        {
            string mode = PaidCheckBox.IsChecked ? "PAID" : "FREE";
            await SecureStorage.Default.SetAsync($"{provider}_Mode", mode);
        }

        StatusLabel.Text = $"Klíč pro {provider} uložen.";
        LoadConnections();
    }

    // ===== SMAZÁNÍ =====
    private async void OnDeleteKeyClicked(object sender, EventArgs e)
    {
        string provider = ProviderPicker.SelectedItem?.ToString() ?? "Gemini";

        bool confirm = await DisplayAlertAsync(
            "Smazat",
            $"Opravdu smazat připojení pro {provider}?",
            "Ano", "Ne");

        if (!confirm) return;

        SecureStorage.Default.Remove(provider);
        SecureStorage.Default.Remove($"{provider}_Mode");

        ApiKeyEntry.Text = "";
        PaidCheckBox.IsChecked = false;

        StatusLabel.Text = $"Připojení {provider} smazáno.";
        LoadConnections();
    }
}
