using NativniLogickaHra.Models;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.View;

public partial class SettingsPage : ContentPage
{
    private bool isInitializing = false;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        isInitializing = true;

        // ensure named controls from XAML are available even if generated fields are missing
        RequiredConsonantsSlider = this.FindByName<Slider>("RequiredConsonantsSlider");
        RequiredConsonantsLabel = this.FindByName<Label>("RequiredConsonantsLabel");

        LanguagePicker.SelectedIndex =
            Preferences.Default.Get("Language", 0);

        VolumeSlider.Value =
            Preferences.Default.Get("Volume", 50);

        DifficultyPicker.SelectedIndex =
            Preferences.Default.Get("Difficulty", 1);

        LivesEnabledSwitch.IsToggled =
            Preferences.Default.Get("LivesEnabled", true);

        LivesSlider.IsEnabled = LivesEnabledSwitch.IsToggled;
        LivesSlider.Value =
            Preferences.Default.Get("LivesCount", 5);

        RequiredConsonantsSlider.Value =
            Preferences.Default.Get("RequiredConsonants", 3);

        UpdateLabels();
        UpdateAiStats();   // ← zobrazí statistiky při každém otevření stránky

        isInitializing = false;
    }

    // ── Pomocné aktualizace ──────────────────────────────────────────────────

    private void UpdateLabels()
    {
        VolumeLabel.Text = $"Hlasitost: {Math.Round(VolumeSlider.Value)} %";

        LivesLabel.Text = LivesEnabledSwitch.IsToggled
            ? $"Počet životů: {Math.Round(LivesSlider.Value)}"
            : "Počet životů: ∞";

        RequiredConsonantsLabel.Text =
            $"Souhlásky pro povolení samohlásek: {Math.Round(RequiredConsonantsSlider.Value)}";
    }

    private void UpdateAiStats()
    {
        // Úroveň jako text s barvou
        (string text, Color color) = PlayerStats.Difficulty switch
        {
            1 => ("Easy 🟢", Color.FromArgb("#27AE60")),
            2 => ("Medium 🟡", Color.FromArgb("#F39C12")),
            _ => ("Hard 🔴", Color.FromArgb("#E74C3C"))
        };

        AiDifficultyLabel.Text = text;
        AiDifficultyLabel.TextColor = color;

        StatsWinsLabel.Text = PlayerStats.Wins.ToString();
        StatsLossesLabel.Text = PlayerStats.Losses.ToString();
        StatsStreakLabel.Text = PlayerStats.Streak.ToString();
        StatsWinRateLabel.Text = PlayerStats.TotalGames == 0
            ? "–"
            : $"{PlayerStats.WinRate:P0}";
    }

    // ── Handlery sliderů / switchů ───────────────────────────────────────────

    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        VolumeLabel.Text = $"Hlasitost: {Math.Round(e.NewValue)} %";
    }

    private void OnLivesChanged(object sender, ValueChangedEventArgs e)
    {
        LivesLabel.Text = $"Počet životů: {Math.Round(e.NewValue)}";
    }

    private void OnLivesToggled(object sender, ToggledEventArgs e)
    {
        LivesSlider.IsEnabled = e.Value;

        if (!e.Value)
        {
            LivesSlider.Value = 0;
            LivesLabel.Text = "Počet životů: ∞";
        }
    }

    public void OnRequiredConsonantsChanged(object sender, ValueChangedEventArgs e)
    {
        RequiredConsonantsLabel.Text =
            $"Souhlásky pro povolení samohlásek: {Math.Round(e.NewValue)}";
    }

    // ── Reset statistik ──────────────────────────────────────────────────────

    private async void OnResetStatsClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Resetovat statistiky",
            "Opravdu chceš smazat všechny statistiky a vrátit obtížnost na Easy?",
            "Ano, resetovat", "Zrušit");

        if (!confirm) return;

        PlayerStats.Reset();
        UpdateAiStats();

        await DisplayAlert("Hotovo", "Statistiky byly resetovány.", "OK");
    }

    // ── Uložení a obtížnostní předvolby ─────────────────────────────────────

    private void OnDifficultyChanged(object sender, EventArgs e)
    {
        if (DifficultyPicker == null || isInitializing) return;

        var diff = (Difficulty)DifficultyPicker.SelectedIndex;

        switch (diff)
        {
            case Difficulty.Easy:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 8;
                break;

            case Difficulty.Normal:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 5;
                break;

            case Difficulty.Hard:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 3;
                break;

            case Difficulty.Custom:
                break;
        }

        UpdateLabels();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set("Language", LanguagePicker.SelectedIndex);
        Preferences.Default.Set("Volume", (int)VolumeSlider.Value);
        Preferences.Default.Set("Difficulty", DifficultyPicker.SelectedIndex);

        Preferences.Default.Set("LivesEnabled", LivesEnabledSwitch.IsToggled);
        Preferences.Default.Set("LivesCount",
            LivesEnabledSwitch.IsToggled ? (int)LivesSlider.Value : 0);

        Preferences.Default.Set("RequiredConsonants", (int)RequiredConsonantsSlider.Value);

        await Navigation.PopAsync();
    }
}